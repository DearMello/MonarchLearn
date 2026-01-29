using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MonarchLearn.Application.Interfaces.Services
{
    public interface IFileService
    {
  
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task<(string FilePath, int Duration)> UploadVideoWithDurationAsync(IFormFile file);
        void DeleteFile(string filePath);
        string GetFullUrl(string relativePath);
    }
}