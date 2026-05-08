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
        EnsureMailOptionsConfigured();

        MimeMessage email = new();
        email.Sender = new MailboxAddress(_mailOptions.DisplayName, _mailOptions.Mail);
        email.From.Add(new MailboxAddress(_mailOptions.DisplayName, _mailOptions.Mail));
        email.To.Add(MailboxAddress.Parse(mailContent.To));
        email.Subject = mailContent.Subject;

        BodyBuilder builder = new();
        builder.HtmlBody = mailContent.Body;
        email.Body = builder.ToMessageBody();

        using SmtpClient smtp = new();
        await smtp.ConnectAsync(_mailOptions.Host, _mailOptions.Port, SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailOptions.Mail, _mailOptions.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
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
