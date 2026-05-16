namespace El_Sim.Web.Models;

public class RegisterViewModel
{
    [Required]
    [StringLength(25, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^[A-Za-z0-9]{7}$")]
    public string Fin { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
