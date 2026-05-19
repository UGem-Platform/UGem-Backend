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
        builder.TextBody = mailContent.Body;
        email.Body = builder.ToMessageBody();

        Console.WriteLine($"HOST: {_mailOptions.Host}");
        Console.WriteLine($"PORT: {_mailOptions.Port}");

        using SmtpClient smtp = new();

        Console.WriteLine("CONNECTING SMTP...");

        await smtp.ConnectAsync(_mailOptions.Host, _mailOptions.Port, SecureSocketOptions.SslOnConnect);

        Console.WriteLine("CONNECTED");

        Console.WriteLine("AUTHENTICATING...");

        await smtp.AuthenticateAsync(
            _mailOptions.Mail,
            _mailOptions.Password);

        Console.WriteLine("AUTH SUCCESS");

        await smtp.SendAsync(email);

        Console.WriteLine("MAIL SENT");

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
