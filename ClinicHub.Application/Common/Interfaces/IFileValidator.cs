using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IFileValidator
    {
        public Task<(bool Uploaded, string Result)> UploadFile(IFormFile file, int Place);
        public bool FileIsExisted(string? FullFilePath);
        public Task<bool> DeleteFile(string fileName, int Place);

        public string GetUniqueFileName(string fileName);
        public bool IsValidFile(string FileName, string PlaceHolder);
        public bool IsValidFile(IFormFile file);
        public Task<(bool Success, string Result)> DownloadFile(int FilePlace, string FileName);
        public Task<(bool Uploaded, string Result)> UploadMultipleFile(List<IFormFile> files, int Place);
    }
}
