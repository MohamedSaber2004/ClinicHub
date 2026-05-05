using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;
using ClinicHub.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Infrastructure.Services.Attachements
{
    public class VideoValidator : IVideoValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public VideoValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadVideo(IFormFile file, int Place)
        {
            if (!IsValidVideo(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(Place));
            if (uploaded)
            {
                return (true, $"{Place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleVideo(List<IFormFile> files, int Place)
        {
            if (files == null || !files.Any())
                return (false, _localizer["Attachments:NoMediaProvided"]);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadVideo(file, Place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (!results.Any())
                return (false, _localizer["Attachments:UploadFailed"]);

            return (true, string.Join(",", results));
        }

        public bool VideoIsExisted(string? FullVideoPath)
        {
            return _baseFileService.FileExists(FullVideoPath);
        }

        public async Task<bool> DeleteVideo(string fileName, int Place)
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

        public bool IsValidVideo(string VideoName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(VideoName) && VideoName != PlaceHolder;
        }

        public bool IsValidVideo(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place)!;
        }
    }
}
