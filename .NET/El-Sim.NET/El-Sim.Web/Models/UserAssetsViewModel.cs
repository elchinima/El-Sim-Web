namespace El_Sim.Web.Models;

public class UserAssetsViewModel
{
    public string? BasicNumber { get; set; }
    public string? GlobalNumber { get; set; }
    public string? Pass { get; set; }
    public string? BasicTariff { get; set; }
    public string? GlobalTariff { get; set; }
    public string? WiFi { get; set; }

    public bool HasAnyValue =>
        !string.IsNullOrWhiteSpace(BasicNumber)
        || !string.IsNullOrWhiteSpace(GlobalNumber)
        || !string.IsNullOrWhiteSpace(Pass)
        || !string.IsNullOrWhiteSpace(BasicTariff)
        || !string.IsNullOrWhiteSpace(GlobalTariff)
        || !string.IsNullOrWhiteSpace(WiFi);
}
