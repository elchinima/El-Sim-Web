namespace El_Sim.Web.Models;

public class VerifyTwoFactorViewModel
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [RegularExpression("^[0-9]{7}$")]
    public string Code { get; set; } = string.Empty;
}
