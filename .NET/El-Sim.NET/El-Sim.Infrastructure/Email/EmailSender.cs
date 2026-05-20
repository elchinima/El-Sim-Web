namespace El_Sim.Infrastructure.Email;

public class EmailSender
{
    private readonly EmailOptions _options;

    public EmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = false)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.From))
        {
            throw new InvalidOperationException("Email settings are not configured.");
        }

        using var message = new MailMessage(_options.From, to, subject, body)
        {
            IsBodyHtml = isHtml
        };
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        var userName = string.IsNullOrWhiteSpace(_options.UserName) ? _options.From : _options.UserName;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            client.Credentials = new NetworkCredential(userName, _options.Password);
        }

        await client.SendMailAsync(message);
    }
}
