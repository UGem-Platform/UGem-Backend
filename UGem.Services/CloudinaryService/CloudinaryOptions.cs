using System.ComponentModel.DataAnnotations;

namespace UGem.Services.CloudinaryService;

public record CloudinaryOptions
{
    [Required] public string CloudName { get; set; } = "dmvb7vbyt";
    [Required] public string ApiKey { get; set; } = "566938632769158";
    [Required] public string ApiSecret { get; set; } = "qw1DUWe8dQVajLS46LJOPLIEkCc";
}
