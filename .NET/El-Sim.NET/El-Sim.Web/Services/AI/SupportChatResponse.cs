namespace El_Sim.Web.Services.AI;

public class SupportChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public bool CloseChat { get; set; }
    public int CloseAfterSeconds { get; set; }
}
