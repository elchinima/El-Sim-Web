namespace El_Sim.Web.Services.AI;

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
