namespace El_Sim.Web.Models;

public class AuthPageViewModel
{
    public LoginViewModel Login { get; set; } = new();
    public RegisterViewModel Register { get; set; } = new();
    public string ActiveForm { get; set; } = "login";
}
