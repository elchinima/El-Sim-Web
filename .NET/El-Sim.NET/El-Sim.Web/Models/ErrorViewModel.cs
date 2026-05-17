namespace El_Sim.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public int StatusCode { get; set; } = StatusCodes.Status500InternalServerError;

    public string Title { get; set; } = "Connection interrupted";

    public string Message { get; set; } = "Something went wrong while processing your request.";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
