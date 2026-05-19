using System.Net.Http.Json;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace UGem.Services.MailService;

public class Service : IService
{
    private readonly MailOption.MailOptions _mailOptions;

    public Service(IOptions<MailOption.MailOptions> mailOptions)
    {
        _mailOptions = mailOptions.Value;
    }

    public async Task SendMail(MailContext mailContent)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", _mailOptions.ApiKey);

        var body = new
        {
            sender = new { name = _mailOptions.DisplayName, email = _mailOptions.Mail },
            to = new[] { new { email = mailContent.To } },
            subject = mailContent.Subject,
            htmlContent = mailContent.Body
        };

        var response = await client.PostAsJsonAsync(
            "https://api.brevo.com/v3/smtp/email", body);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Failed to send email via Brevo API");
    }

    private void EnsureMailOptionsConfigured()
    {
        if (!HasConfiguredValue(_mailOptions.Mail)
            || !HasConfiguredValue(_mailOptions.DisplayName)
            || !HasConfiguredValue(_mailOptions.Password)
            || !HasConfiguredValue(_mailOptions.Host)
            || _mailOptions.Port <= 0)
        {
            throw new InvalidOperationException("Mail service is not configured.");
        }
    }

    private static bool HasConfiguredValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return !value.Contains("__SET", StringComparison.OrdinalIgnoreCase)
               && !value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
               && !value.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase);
    }
}
