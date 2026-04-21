using System.ComponentModel.DataAnnotations;

namespace UGem.Services.CloudinaryService;

public record CloudinaryOptions
{
    [Required]public string CloudName { get; set; } = string.Empty;
    [Required]public string ApiKey { get; set; } = string.Empty;
    [Required]public string ApiSecret { get; set; } = string.Empty;
}
