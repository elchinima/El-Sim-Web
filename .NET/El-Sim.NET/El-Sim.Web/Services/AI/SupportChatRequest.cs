namespace El_Sim.Web.Services.AI;

public class SupportChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessage> History { get; set; } = [];
    public string Language { get; set; } = "en";
}
