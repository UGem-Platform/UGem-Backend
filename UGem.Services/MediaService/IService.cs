using Microsoft.AspNetCore.Http;

namespace UGem.Service.MediaService;

public interface IService
{
    public Task<string> UploadImageAsync(IFormFile file);
}