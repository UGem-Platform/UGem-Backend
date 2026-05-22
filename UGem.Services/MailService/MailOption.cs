using System.ComponentModel.DataAnnotations;

namespace UGem.Services.MailService;

public class MailOption
{
    public class MailOptions
    {
        [Required] public string Mail { get; set; } = string.Empty;
        [Required] public string DisplayName { get; set; } = string.Empty;
        [Required] public string ApiKey { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; }
    }
}
