using Microsoft.AspNetCore.Http;

namespace UGem.Services.MediaService;

public interface IService
{
    public Task<string> UploadImageAsync(IFormFile file);
}