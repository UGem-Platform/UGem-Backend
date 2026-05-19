using System.ComponentModel.DataAnnotations;

namespace UGem.Services.MailService;

public class MailOption
{
    public class MailOptions
    {
        [Required] public string Mail { get; set; } = string.Empty;
        [Required] public string DisplayName { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        [Required] public string Host { get; set; } = string.Empty;
        [Required] public int Port { get; set; }
        public string? ApiKey { get; set; }
    }
}
