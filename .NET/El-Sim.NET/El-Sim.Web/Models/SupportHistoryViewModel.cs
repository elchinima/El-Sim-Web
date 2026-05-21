namespace El_Sim.Web.Models;

public class SupportHistoryViewModel
{
    public List<SupportHistoryItemViewModel> Chats { get; set; } = [];
}

public class SupportHistoryItemViewModel
{
    public int Id { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public List<SupportHistoryMessageViewModel> Messages { get; set; } = [];
}

public class SupportHistoryMessageViewModel
{
    public string Role { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
