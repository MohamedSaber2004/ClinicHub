using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;
using ClinicHub.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Infrastructure.Services.Attachements
{
    public class FileValidator : IFileValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public FileValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadFile(IFormFile file, int Place)
        {
            if (!IsValidFile(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(Place));
            if (uploaded)
            {
                return (true, $"{Place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public bool FileIsExisted(string? FullFilePath)
        {
            return _baseFileService.FileExists(FullFilePath);
        }

        public async Task<bool> DeleteFile(string fileName, int Place)
        {
            if (fileName.Contains('_'))
            {
                var parts = fileName.Split('_');
                if (int.TryParse(parts[0], out _))
                {
                    fileName = string.Join("_", parts.Skip(1));
                }
            }
            return await _baseFileService.DeleteFileAsync(fileName, GetFolderPath(Place));
        }

        public string GetUniqueFileName(string fileName)
        {
            return _baseFileService.GetUniqueFileName(fileName);
        }

        public bool IsValidFile(string FileName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(FileName) && FileName != PlaceHolder;
        }

        public bool IsValidFile(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".zip", ".rar" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        public async Task<(bool Success, string Result)> DownloadFile(int FilePlace, string FileName)
        {
            if (FileName.Contains('_'))
            {
                var parts = FileName.Split('_');
                if (int.TryParse(parts[0], out _))
                {
                    FileName = string.Join("_", parts.Skip(1));
                }
            }
            return await _baseFileService.DownloadFileAsync(GetFolderPath(FilePlace), FileName);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleFile(List<IFormFile> files, int Place)
        {
            if (files == null || !files.Any())
                return (false, _localizer["Attachments:NoMediaProvided"]);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadFile(file, Place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (!results.Any())
                return (false, _localizer["Attachments:UploadFailed"]);

            return (true, string.Join(",", results));
        }

        private string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place)!;
        }
    }
}
