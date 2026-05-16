namespace El_Sim.Web.Models;

public class LoginViewModel
{
    [Required]
    [RegularExpression("^[A-Za-z0-9]{7}$")]
    public string Fin { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
