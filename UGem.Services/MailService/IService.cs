namespace UGem.Service.MailService;

public interface IService
{
    public Task SendMail(MailContext mailContent);
}

public class MailContext
{
    public required string To { get; set; } //Địa chỉ gừi đến
    public required string Subject { get; set; } // Chủ đề (tiêu đề mail)
    public required string Body { get; set; } // Nội dung (hỗ trợ HTML ) cảu email
}
