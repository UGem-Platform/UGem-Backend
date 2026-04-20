namespace UGem.Service.MailService;

public interface IService
{
    public Task SendMail(MailContext mailContext);
}

public class MailContext
{
    public required string To { get; set; } 
    public required string Subject { get; set; } 
    public required string Body { get; set; } 
}