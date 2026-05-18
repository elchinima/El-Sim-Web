namespace El_Sim.Persistence.Entities;

public class UserAssets
{
    public int Id { get; set; }
    public string? BasicNumber { get; set; }
    public string? GlobalNumber { get; set; }
    public string? Pass { get; set; }
    public string? BasicTariff { get; set; }
    public string? GlobalTariff { get; set; }
    public string? WiFi { get; set; }
    public AppUser? User { get; set; }
}
