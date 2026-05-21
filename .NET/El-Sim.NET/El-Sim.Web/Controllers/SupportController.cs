namespace El_Sim.Web.Controllers;

[Route("support")]
public class SupportController : Controller
{
    private static readonly string[] SupportAgentNames = ["Aylin", "Sevda", "Leyla"];
    private static readonly JsonSerializerOptions SessionJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IntentDetector _intentDetector;
    private readonly ContextBuilder _contextBuilder;
    private readonly GeminiService _geminiService;
    private readonly ElSimDbContext _dbContext;
    private readonly SupportImageProcessor _supportImageProcessor;
    private readonly IWebHostEnvironment _environment;

    public SupportController(
        IntentDetector intentDetector,
        ContextBuilder contextBuilder,
        GeminiService geminiService,
        ElSimDbContext dbContext,
        SupportImageProcessor supportImageProcessor,
        IWebHostEnvironment environment)
    {
        _intentDetector = intentDetector;
        _contextBuilder = contextBuilder;
        _geminiService = geminiService;
        _dbContext = dbContext;
        _supportImageProcessor = supportImageProcessor;
        _environment = environment;
    }

    [Authorize]
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SupportChatRequest? request)
    {
        var message = request?.Message?.Trim() ?? string.Empty;
        var attachedImage = request?.ImageId is int imageId
            ? await GetPendingImage(imageId)
            : null;

        if (string.IsNullOrWhiteSpace(message) && attachedImage is null)
        {
            return BadRequest(new { reply = "Message is required.", agentName = GetAgentName(), closeChat = false });
        }

        var activeChat = await GetActiveChat();
        var agentName = activeChat?.AgentName ?? GetAgentName();
        if (activeChat is not null)
        {
            HttpContext.Session.SetInt32("SupportChatId", activeChat.Id);
            HttpContext.Session.SetString("SupportAgentName", activeChat.AgentName);
        }
        var language = NormalizeLanguage(request?.Language);
        var sessionHistory = GetSessionHistory();
        var history = sessionHistory.Count == 0 ? NormalizeHistory(request?.History) : sessionHistory;

        var aiMessage = string.IsNullOrWhiteSpace(message) ? "User sent an image." : message;

        if (!string.IsNullOrWhiteSpace(message) && IsBotQuestion(message))
        {
            var count = (HttpContext.Session.GetInt32("BotQuestionCount") ?? 0) + 1;
            HttpContext.Session.SetInt32("BotQuestionCount", count);

            var closeChat = count >= 5;
            var reply = closeChat
                ? GetCloseChatReply(language)
                : GetBotQuestionReply(language, agentName, count);

            await SaveChatAsync(history, message, reply, agentName, attachedImage, closeChat);

            return Json(new SupportChatResponse
            {
                Reply = reply,
                AgentName = agentName,
                CloseChat = closeChat,
                CloseAfterSeconds = closeChat ? 60 : 0,
                ImageUrl = attachedImage?.FilePath
            });
        }

        HttpContext.Session.SetInt32("BotQuestionCount", 0);

        var intent = _intentDetector.Detect(aiMessage);
        var prompt = await _contextBuilder.BuildAsync(intent, agentName);
        var geminiHistory = history.TakeLast(10).ToList();
        var geminiReply = await _geminiService.SendAsync(prompt, geminiHistory, aiMessage);
        var hasConnectionError = string.Equals(geminiReply, GeminiService.ConnectionErrorReply, StringComparison.Ordinal);
        var shouldCloseAfterFarewell = !string.IsNullOrWhiteSpace(message) && IsFarewell(message);
        await SaveChatAsync(history, message, hasConnectionError ? string.Empty : geminiReply, agentName, attachedImage, shouldCloseAfterFarewell);

        return Json(new SupportChatResponse
        {
            Reply = hasConnectionError ? string.Empty : geminiReply,
            AgentName = agentName,
            CloseChat = shouldCloseAfterFarewell,
            CloseAfterSeconds = shouldCloseAfterFarewell ? 60 : 0,
            ImageUrl = attachedImage?.FilePath,
            HasConnectionError = hasConnectionError
        });
    }

    [Authorize]
    [HttpPost("image")]
    [RequestSizeLimit(3_200_000)]
    public async Task<IActionResult> UploadImage(IFormFile? image)
    {
        if (image is null || image.Length == 0)
        {
            return BadRequest(new { message = "Image is required." });
        }

        if (image.Length > 3 * 1024 * 1024)
        {
            return BadRequest(new { message = "Image must be up to 3 MB." });
        }

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png")
        {
            return BadRequest(new { message = "Only jpeg, jpg and png images are allowed." });
        }

        var uploadDirectory = Path.Combine(_environment.WebRootPath, "Uploads", "Support");
        Directory.CreateDirectory(uploadDirectory);

        var fileName = $"{Guid.NewGuid():N}.webp";
        var relativePath = $"/Uploads/Support/{fileName}";
        var outputPath = Path.Combine(uploadDirectory, fileName);

        try
        {
            await using var input = image.OpenReadStream();
            await _supportImageProcessor.SaveWebpAsync(input, outputPath, HttpContext.RequestAborted);
        }
        catch
        {
            if (System.IO.File.Exists(outputPath))
            {
                System.IO.File.Delete(outputPath);
            }

            return BadRequest(new { message = "Image could not be processed." });
        }

        var supportImage = new SupportChatImage
        {
            UserId = GetCurrentUserId(),
            SessionId = GetSessionId(),
            FilePath = relativePath,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.SupportChatImages.Add(supportImage);
        await _dbContext.SaveChangesAsync();

        return Json(new { id = supportImage.Id, url = relativePath });
    }

    [Authorize]
    [HttpDelete("image/{id:int}")]
    public async Task<IActionResult> DeleteImage(int id)
    {
        var image = await GetPendingImage(id);

        if (image is null)
        {
            return NotFound();
        }

        DeleteImageFile(image.FilePath);
        _dbContext.SupportChatImages.Remove(image);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("chats")]
    public async Task<IActionResult> Chats()
    {
        var userId = GetCurrentUserId();
        var sessionId = GetSessionId();
        var chats = await _dbContext.SupportChats
            .AsNoTracking()
            .Include(item => item.Messages)
                .ThenInclude(message => message.Images)
            .Where(item => item.IsClosed
                && item.Messages.Any()
                && ((userId != null && item.UserId == userId) || item.SessionId == sessionId))
            .OrderByDescending(item => item.CreatedAtUtc)
            .Select(item => new SupportHistoryItemViewModel
            {
                Id = item.Id,
                AgentName = item.AgentName,
                CreatedAtUtc = item.CreatedAtUtc,
                Messages = item.Messages
                    .OrderBy(message => message.CreatedAtUtc)
                    .Select(message => new SupportHistoryMessageViewModel
                    {
                        Role = message.Role,
                        Text = message.Text,
                        ImageUrl = message.Images.Select(image => image.FilePath).FirstOrDefault(),
                        CreatedAtUtc = message.CreatedAtUtc
                    })
                    .ToList()
            })
            .ToListAsync();

        return View("~/Views/Home/SupportHistory.cshtml", new SupportHistoryViewModel { Chats = chats });
    }

    [Authorize]
    [HttpPost("close")]
    public async Task<IActionResult> Close()
    {
        await CloseCurrentChat();

        return Ok();
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> History()
    {
        var activeChat = await GetActiveChat();
        var dbHistory = activeChat is not null
            ? await GetChatHistory(activeChat.Id)
            : [];

        return Json(new
        {
            agentName = activeChat?.AgentName ?? GetAgentName(),
            history = dbHistory.Count > 0 ? dbHistory : GetFullSessionHistory(),
            isActive = activeChat is not null,
            isClosed = activeChat?.IsClosed ?? false
        });
    }

    private string GetAgentName()
    {
        var agentName = HttpContext.Session.GetString("SupportAgentName");

        if (!string.IsNullOrWhiteSpace(agentName))
        {
            return agentName;
        }

        agentName = SupportAgentNames[RandomNumberGenerator.GetInt32(SupportAgentNames.Length)];
        HttpContext.Session.SetString("SupportAgentName", agentName);
        HttpContext.Session.SetInt32("BotQuestionCount", 0);

        return agentName;
    }

    private List<ChatMessage> GetSessionHistory()
    {
        var json = HttpContext.Session.GetString("SupportChatHistory");

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ChatMessage>>(json, SessionJsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private List<ChatMessage> GetFullSessionHistory()
    {
        var json = HttpContext.Session.GetString("SupportChatFullHistory");

        if (string.IsNullOrWhiteSpace(json))
        {
            return GetSessionHistory();
        }

        try
        {
            return JsonSerializer.Deserialize<List<ChatMessage>>(json, SessionJsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return GetSessionHistory();
        }
    }

    private async Task SaveChatAsync(List<ChatMessage> history, string userMessage, string reply, string agentName, SupportChatImage? image, bool closeChat)
    {
        history.Add(new ChatMessage { Role = "user", Text = userMessage });
        history.Add(new ChatMessage { Role = "model", Text = reply });

        var trimmed = history
            .Where(item => !string.IsNullOrWhiteSpace(item.Text))
            .TakeLast(10)
            .ToList();

        var fullHistory = GetFullSessionHistory();
        fullHistory.Add(new ChatMessage { Role = "user", Text = userMessage });
        fullHistory.Add(new ChatMessage { Role = "model", Text = reply });
        fullHistory = fullHistory
            .Where(item => !string.IsNullOrWhiteSpace(item.Text))
            .TakeLast(100)
            .ToList();

        HttpContext.Session.SetString("SupportChatHistory", JsonSerializer.Serialize(trimmed, SessionJsonOptions));
        HttpContext.Session.SetString("SupportChatFullHistory", JsonSerializer.Serialize(fullHistory, SessionJsonOptions));

        var chat = await GetOrCreateChat(agentName);
        var now = DateTime.UtcNow;
        var userMessageEntity = new SupportChatMessage
        {
            SupportChatId = chat.Id,
            Role = "user",
            Text = userMessage,
            CreatedAtUtc = now
        };

        _dbContext.SupportChatMessages.Add(userMessageEntity);
        await _dbContext.SaveChangesAsync();

        if (image is not null)
        {
            image.SupportChatMessageId = userMessageEntity.Id;
        }

        _dbContext.SupportChatMessages.Add(new SupportChatMessage
        {
            SupportChatId = chat.Id,
            Role = "model",
            Text = reply,
            CreatedAtUtc = DateTime.UtcNow
        });
        chat.UpdatedAtUtc = DateTime.UtcNow;
        chat.IsClosed = closeChat;
        await _dbContext.SaveChangesAsync();
    }

    private static List<ChatMessage> NormalizeHistory(List<ChatMessage>? history)
    {
        return (history ?? [])
            .Where(message => !string.IsNullOrWhiteSpace(message.Text))
            .Select(message => new ChatMessage
            {
                Role = string.Equals(message.Role, "model", StringComparison.OrdinalIgnoreCase) ? "model" : "user",
                Text = message.Text.Trim()
            })
            .TakeLast(10)
            .ToList();
    }

    private static bool IsBotQuestion(string message)
    {
        var normalized = message.ToLowerInvariant();

        if (Regex.IsMatch(normalized, @"(^|[^\p{L}\p{N}])(bot|ai|robot)([^\p{L}\p{N}]|$)", RegexOptions.IgnoreCase))
        {
            return true;
        }

        return normalized.Contains("b\u043e\u0442", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0431\u043e\u0442", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0438\u0438", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("s\u00fcni", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("suni", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("insan deyil", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0447\u0435\u043b\u043e\u0432\u0435\u043a", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0440\u043e\u0431\u043e\u0442", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFarewell(string message)
    {
        var normalized = message.Trim().ToLowerInvariant();

        if (Regex.IsMatch(normalized, @"(^|[^\p{L}\p{N}])(bye|goodbye|thanks|thank you|thx|see you)([^\p{L}\p{N}]|$)", RegexOptions.IgnoreCase))
        {
            return true;
        }

        return normalized.Contains("\u043f\u043e\u043a\u0430", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0434\u043e \u0441\u0432\u0438\u0434\u0430\u043d\u0438\u044f", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0441\u043f\u0430\u0441\u0438\u0431\u043e", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("\u0441\u0430\u0433\u043e\u043b", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("sagol", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("sa\u011f ol", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("sa\u011fol", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("t\u0259\u015f\u0259kk\u00fcr", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("tesekkur", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("sa\u011fl\u0131qla", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("sagliqla", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetBotQuestionReply(string language, string agentName, int count)
    {
        return language switch
        {
            "ru" => count switch
            {
                1 => $"\u0421 \u0432\u0430\u043c\u0438 \u0440\u0430\u0437\u0433\u043e\u0432\u0430\u0440\u0438\u0432\u0430\u0435\u0442 {agentName}. \u0427\u0435\u043c \u044f \u043c\u043e\u0433\u0443 \u043f\u043e\u043c\u043e\u0447\u044c \u043f\u043e El-Sim?",
                2 => $"{agentName} \u043d\u0430 \u0441\u0432\u044f\u0437\u0438. \u0414\u0430\u0432\u0430\u0439\u0442\u0435 \u043b\u0443\u0447\u0448\u0435 \u0440\u0435\u0448\u0438\u043c \u0432\u0430\u0448 \u0432\u043e\u043f\u0440\u043e\u0441 \u043f\u043e \u0442\u0430\u0440\u0438\u0444\u0443, eSIM \u0438\u043b\u0438 \u0430\u043a\u0442\u0438\u0432\u0430\u0446\u0438\u0438.",
                3 => "\u042f \u0437\u0434\u0435\u0441\u044c, \u0447\u0442\u043e\u0431\u044b \u043f\u043e\u043c\u043e\u0447\u044c \u0441 \u043f\u0440\u043e\u0434\u0443\u043a\u0442\u0430\u043c\u0438 \u0438 \u043f\u043e\u0434\u0434\u0435\u0440\u0436\u043a\u043e\u0439 El-Sim. \u041e\u043f\u0438\u0448\u0438\u0442\u0435 \u043f\u0440\u043e\u0431\u043b\u0435\u043c\u0443, \u0438 \u043c\u044b \u0440\u0430\u0437\u0431\u0435\u0440\u0435\u043c \u0435\u0435 \u0448\u0430\u0433 \u0437\u0430 \u0448\u0430\u0433\u043e\u043c.",
                _ => "\u0414\u0430\u0432\u0430\u0439\u0442\u0435 \u043f\u0440\u043e\u0434\u043e\u043b\u0436\u0438\u043c \u0442\u043e\u043b\u044c\u043a\u043e \u043f\u043e \u0432\u043e\u043f\u0440\u043e\u0441\u0430\u043c El-Sim. \u041d\u0430\u043f\u0438\u0448\u0438\u0442\u0435, \u0447\u0442\u043e \u043d\u0443\u0436\u043d\u043e \u043f\u0440\u043e\u0432\u0435\u0440\u0438\u0442\u044c."
            },
            "az" => count switch
            {
                1 => $"Sizinl\u0259 {agentName} dan\u0131\u015f\u0131r. El-Sim \u00fczr\u0259 nec\u0259 k\u00f6m\u0259k ed\u0259 bil\u0259r\u0259m?",
                2 => $"{agentName} x\u0259td\u0259dir. G\u0259lin tarif, eSIM v\u0259 ya aktivasiya sual\u0131n\u0131z\u0131 h\u0259ll ed\u0259k.",
                3 => "M\u0259n El-Sim m\u0259hsullar\u0131 v\u0259 d\u0259st\u0259yi \u00fczr\u0259 k\u00f6m\u0259k etm\u0259k \u00fc\u00e7\u00fcn buradayam. Problemi yaz\u0131n, add\u0131m-add\u0131m baxaq.",
                _ => "G\u0259lin yaln\u0131z El-Sim suallar\u0131 \u00fczr\u0259 davam ed\u0259k. N\u0259yi yoxlamaq laz\u0131md\u0131rsa yaz\u0131n."
            },
            _ => count switch
            {
                1 => $"You are chatting with {agentName}. How can I help with El-Sim?",
                2 => $"{agentName} is here. Let us focus on your plan, eSIM, or activation question.",
                3 => "I am here to help with El-Sim products and support. Tell me what happened, and we will handle it step by step.",
                _ => "Let us continue with El-Sim support questions only. Tell me what needs checking."
            }
        };
    }

    private static string GetCloseChatReply(string language)
    {
        return language switch
        {
            "ru" => "\u0427\u0430\u0442 \u0437\u0430\u043a\u0440\u044b\u0442 \u043d\u0430 60 \u0441\u0435\u043a\u0443\u043d\u0434 \u0438\u0437-\u0437\u0430 \u043f\u043e\u0432\u0442\u043e\u0440\u044f\u044e\u0449\u0438\u0445\u0441\u044f \u0432\u043e\u043f\u0440\u043e\u0441\u043e\u0432 \u043d\u0435 \u043f\u043e \u0442\u0435\u043c\u0435.",
            "az" => "M\u00f6vzudan k\u0259nar t\u0259krarlanan suallara g\u00f6r\u0259 \u00e7at 60 saniy\u0259lik ba\u011fland\u0131.",
            _ => "Chat is closed for 60 seconds because of repeated off-topic questions."
        };
    }

    private static string NormalizeLanguage(string? language)
    {
        return language?.Trim().ToLowerInvariant() switch
        {
            "ru" => "ru",
            "az" => "az",
            _ => "en"
        };
    }

    private int? GetCurrentUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }

    private string GetSessionId()
    {
        var sessionId = HttpContext.Session.GetString("SupportSessionId");

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return sessionId;
        }

        sessionId = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        HttpContext.Session.SetString("SupportSessionId", sessionId);

        return sessionId;
    }

    private async Task<SupportChat> GetOrCreateChat(string agentName)
    {
        var chatId = HttpContext.Session.GetInt32("SupportChatId");

        if (chatId is int existingChatId)
        {
            var existingChat = await _dbContext.SupportChats.FirstOrDefaultAsync(item => item.Id == existingChatId);

            if (existingChat is not null && !existingChat.IsClosed)
            {
                return existingChat;
            }
        }

        var activeChat = await GetActiveChat();

        if (activeChat is not null)
        {
            HttpContext.Session.SetInt32("SupportChatId", activeChat.Id);
            HttpContext.Session.SetString("SupportAgentName", activeChat.AgentName);

            return activeChat;
        }

        var now = DateTime.UtcNow;
        var chat = new SupportChat
        {
            UserId = GetCurrentUserId(),
            SessionId = GetSessionId(),
            AgentName = agentName,
            IsClosed = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _dbContext.SupportChats.Add(chat);
        await _dbContext.SaveChangesAsync();
        HttpContext.Session.SetInt32("SupportChatId", chat.Id);

        return chat;
    }

    private async Task<SupportChat?> GetActiveChat()
    {
        var userId = GetCurrentUserId();
        var sessionId = HttpContext.Session.GetString("SupportSessionId");
        var chatId = HttpContext.Session.GetInt32("SupportChatId");

        var query = _dbContext.SupportChats.Where(item => !item.IsClosed);

        if (chatId is int existingChatId)
        {
            var sessionChat = await query.FirstOrDefaultAsync(item => item.Id == existingChatId);

            if (sessionChat is not null)
            {
                return sessionChat;
            }
        }

        return await query
            .Where(item => (userId != null && item.UserId == userId) || (!string.IsNullOrWhiteSpace(sessionId) && item.SessionId == sessionId))
            .OrderByDescending(item => item.UpdatedAtUtc)
            .FirstOrDefaultAsync();
    }

    private async Task<List<ChatMessage>> GetChatHistory(int chatId)
    {
        return await _dbContext.SupportChatMessages
            .AsNoTracking()
            .Where(item => item.SupportChatId == chatId)
            .OrderBy(item => item.CreatedAtUtc)
            .Select(item => new ChatMessage
            {
                Role = item.Role,
                Text = item.Text,
                ImageUrl = item.Images.Select(image => image.FilePath).FirstOrDefault()
            })
            .ToListAsync();
    }

    private async Task CloseCurrentChat()
    {
        var chatId = HttpContext.Session.GetInt32("SupportChatId");

        if (chatId is not int existingChatId)
        {
            return;
        }

        var chat = await _dbContext.SupportChats.FirstOrDefaultAsync(item => item.Id == existingChatId);

        if (chat is null)
        {
            return;
        }

        chat.IsClosed = true;
        chat.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        HttpContext.Session.Remove("SupportChatId");
        HttpContext.Session.Remove("SupportChatHistory");
        HttpContext.Session.Remove("SupportChatFullHistory");
    }

    private async Task<SupportChatImage?> GetPendingImage(int id)
    {
        var sessionId = GetSessionId();
        var userId = GetCurrentUserId();

        return await _dbContext.SupportChatImages
            .FirstOrDefaultAsync(item => item.Id == id
                && item.SupportChatMessageId == null
                && item.SessionId == sessionId
                && item.UserId == userId);
    }

    private void DeleteImageFile(string filePath)
    {
        var webRoot = Path.GetFullPath(_environment.WebRootPath);
        var relativePath = filePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));

        if (fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
