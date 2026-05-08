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
}
