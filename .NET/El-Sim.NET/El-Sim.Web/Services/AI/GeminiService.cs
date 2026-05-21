namespace El_Sim.Web.Services.AI;

public class GeminiService
{
    public const string ConnectionErrorReply = "__SUPPORT_CONNECTION_ERROR__";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiOptions _options;

    public GeminiService(IHttpClientFactory httpClientFactory, IOptions<GeminiOptions> options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options.Value;
    }

    public async Task<string> SendAsync(string systemPrompt, List<ChatMessage> history, string userMessage)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || _options.ApiKey.Trim().StartsWith("{{", StringComparison.Ordinal))
        {
            return ConnectionErrorReply;
        }

        var model = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-3.1-flash-lite" : _options.Model.Trim();
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}";
        var contents = history
            .TakeLast(10)
            .Where(message => !string.IsNullOrWhiteSpace(message.Text))
            .Select(message => new GeminiContent
            {
                Role = NormalizeRole(message.Role),
                Parts = [new GeminiPart { Text = message.Text }]
            })
            .ToList();

        contents.Add(new GeminiContent
        {
            Role = "user",
            Parts = [new GeminiPart { Text = userMessage }]
        });

        var request = new GeminiGenerateRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = [new GeminiPart { Text = systemPrompt }]
            },
            Contents = contents
        };

        var client = _httpClientFactory.CreateClient("Gemini");
        using var response = await client.PostAsJsonAsync(endpoint, request);

        if (!response.IsSuccessStatusCode)
        {
            return ConnectionErrorReply;
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiGenerateResponse>();
        var text = result?.Candidates?
            .SelectMany(candidate => candidate.Content?.Parts ?? [])
            .Select(part => part.Text)
            .FirstOrDefault(part => !string.IsNullOrWhiteSpace(part));

        return string.IsNullOrWhiteSpace(text)
            ? ConnectionErrorReply
            : text;
    }

    private static string NormalizeRole(string? role)
    {
        return string.Equals(role, "model", StringComparison.OrdinalIgnoreCase) ? "model" : "user";
    }

    private sealed class GeminiGenerateRequest
    {
        [JsonPropertyName("system_instruction")]
        public GeminiSystemInstruction SystemInstruction { get; set; } = new();

        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];
    }

    private sealed class GeminiSystemInstruction
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private sealed class GeminiGenerateResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }
}
