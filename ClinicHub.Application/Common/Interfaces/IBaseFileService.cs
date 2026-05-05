using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IBaseFileService
    {
        Task<(bool Uploaded, string Result)> UploadFileAsync(IFormFile file, string folderPath);
        bool FileExists(string? fullFilePath);
        Task<bool> DeleteFileAsync(string fileName, string folderPath);
        string GetUniqueFileName(string fileName);
        Task<(bool Success, string Result)> DownloadFileAsync(string folderPath, string fileName);
    }
}
