namespace El_Sim.Persistence.Entities;

public class HomeSlider
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public bool IsMobile { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
