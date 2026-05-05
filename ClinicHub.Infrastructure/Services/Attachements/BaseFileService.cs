using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Infrastructure.Services.Attachements
{
    public class BaseFileService : IBaseFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IStringLocalizer<Messages> _localizer;

        private string WebRootPath => _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

        public BaseFileService(IWebHostEnvironment webHostEnvironment, IStringLocalizer<Messages> localizer)
        {
            _webHostEnvironment = webHostEnvironment;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadFileAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
                return (false, _localizer["Attachments:FileEmpty"]);

            try
            {
                string uploadsFolder = Path.Combine(WebRootPath, folderPath);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = GetUniqueFileName(file.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return (true, Path.Combine(folderPath, uniqueFileName).Replace("\\", "/"));
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public bool FileExists(string? fullFilePath)
        {
            if (string.IsNullOrEmpty(fullFilePath))
                return false;

            string filePath = Path.Combine(WebRootPath, fullFilePath.TrimStart('/'));
            return File.Exists(filePath);
        }

        public async Task<bool> DeleteFileAsync(string fileName, string folderPath)
        {
            try
            {
                string filePath = Path.Combine(WebRootPath, folderPath, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetUniqueFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return Guid.NewGuid().ToString() + extension;
        }

        public async Task<(bool Success, string Result)> DownloadFileAsync(string folderPath, string fileName)
        {
            string filePath = Path.Combine(WebRootPath, folderPath, fileName);

            if (File.Exists(filePath))
            {
                return (true, filePath);
            }

            return (false, _localizer["Attachments:FileNotFound"]);
        }
    }
}
