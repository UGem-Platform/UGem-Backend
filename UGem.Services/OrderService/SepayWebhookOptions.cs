using System.ComponentModel.DataAnnotations;

namespace UGem.Services.OrderService;

public class SepayWebhookOptions
{
    public const string SectionName = "PaymentWebhook";

    [Required]
    public string HeaderName { get; set; } = string.Empty;

    [Required]
    public string SharedSecret { get; set; } = string.Empty;
}
