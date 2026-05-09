using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using UGem.Services.MediaService;

namespace UGem.Services.CloudinaryService;

public class Service : IService
{
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    private readonly Cloudinary _cloudinary;

    public Service(IOptions<CloudinaryOptions> cloudinaryOptions)
    {
        var options = cloudinaryOptions.Value;
        _cloudinary = new Cloudinary(new Account(
            options.CloudName,
            options.ApiKey,
            options.ApiSecret));
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null.", nameof(file));
        }

        if (!IsImageFile(file))
        {
            throw new ArgumentException("File is not a valid image.", nameof(file));
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException("Image size must not exceed 5 MB.");
        }

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream)
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error != null)
        {
            throw new InvalidOperationException(
                $"Cloudinary upload failed: {uploadResult.Error.Message}"
            );
        }

        return uploadResult.SecureUrl.ToString();
    }

    private static bool IsImageFile(IFormFile file)
    {
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var contentType = file.ContentType?.ToLowerInvariant();

        return AllowedExtensions.Contains(fileExtension)
               && !string.IsNullOrWhiteSpace(contentType)
               && AllowedContentTypes.Contains(contentType);
    }
}