using Microsoft.AspNetCore.Http;


namespace Application.Services.FileServices
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string filePath);
    }
}
