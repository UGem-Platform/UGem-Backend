namespace UGem.Service.MailService;

public interface IService
{
    public Task SendMail(MailContext mailContent);
}

public class MailContext
{
    public required string To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; } 
}
