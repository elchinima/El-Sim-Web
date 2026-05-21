namespace El_Sim.Domain.Entities;

public class SupportChat
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public AppUser? User { get; set; }
    public ICollection<SupportChatMessage> Messages { get; set; } = new List<SupportChatMessage>();
}
