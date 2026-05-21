namespace El_Sim.Domain.Entities;

public class SupportChatMessage
{
    public int Id { get; set; }
    public int SupportChatId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public SupportChat SupportChat { get; set; } = new();
    public ICollection<SupportChatImage> Images { get; set; } = new List<SupportChatImage>();
}
