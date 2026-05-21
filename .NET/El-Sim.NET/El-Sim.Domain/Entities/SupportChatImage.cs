namespace El_Sim.Domain.Entities;

public class SupportChatImage
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int? SupportChatMessageId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public AppUser? User { get; set; }
    public SupportChatMessage? SupportChatMessage { get; set; }
}
