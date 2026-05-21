namespace El_Sim.Web.Controllers;

[Route("support")]
public class SupportController : Controller
{
    private static readonly string[] SupportAgentNames = ["Aylin", "Sevda", "Leyla"];
    private static readonly JsonSerializerOptions SessionJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IntentDetector _intentDetector;
    private readonly ContextBuilder _contextBuilder;
    private readonly GeminiService _geminiService;

    public SupportController(
        IntentDetector intentDetector,
        ContextBuilder contextBuilder,
        GeminiService geminiService)
    {
        _intentDetector = intentDetector;
        _contextBuilder = contextBuilder;
        _geminiService = geminiService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SupportChatRequest? request)
    {
        var message = request?.Message?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest(new { reply = "Message is required.", agentName = GetAgentName(), closeChat = false });
        }

        var agentName = GetAgentName();
        var language = NormalizeLanguage(request?.Language);
        var sessionHistory = GetSessionHistory();
        var history = sessionHistory.Count == 0 ? NormalizeHistory(request?.History) : sessionHistory;

        if (IsBotQuestion(message))
        {
            var count = (HttpContext.Session.GetInt32("BotQuestionCount") ?? 0) + 1;
            HttpContext.Session.SetInt32("BotQuestionCount", count);

            var closeChat = count >= 5;
            var reply = closeChat
                ? GetCloseChatReply(language)
                : GetBotQuestionReply(language, agentName, count);

            SaveSessionHistory(history, message, reply);

            return Json(new SupportChatResponse
            {
                Reply = reply,
                AgentName = agentName,
                CloseChat = closeChat,
                CloseAfterSeconds = closeChat ? 60 : 0
            });
        }

        HttpContext.Session.SetInt32("BotQuestionCount", 0);

        var intent = _intentDetector.Detect(message);
        var prompt = await _contextBuilder.BuildAsync(intent, agentName);
        var geminiHistory = history.TakeLast(10).ToList();
        var geminiReply = await _geminiService.SendAsync(prompt, geminiHistory, message);

        SaveSessionHistory(history, message, geminiReply);
        var shouldCloseAfterFarewell = IsFarewell(message);

        return Json(new SupportChatResponse
        {
            Reply = geminiReply,
            AgentName = agentName,
            CloseChat = shouldCloseAfterFarewell,
            CloseAfterSeconds = shouldCloseAfterFarewell ? 60 : 0
        });
    }

    [HttpGet("history")]
    public IActionResult History()
    {
        return Json(new
        {
            agentName = GetAgentName(),
            history = GetFullSessionHistory()
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

    private void SaveSessionHistory(List<ChatMessage> history, string userMessage, string reply)
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
}
