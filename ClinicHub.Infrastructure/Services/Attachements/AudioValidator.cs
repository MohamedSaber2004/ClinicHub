using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;
using ClinicHub.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Infrastructure.Services.Attachements
{
    public class AudioValidator : IAudioValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IStringLocalizer<Messages> _localizer;

        public AudioValidator(IBaseFileService baseFileService, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadAudio(IFormFile file, int Place)
        {
            if (!IsValidAudio(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(Place));
            if (uploaded)
            {
                return (true, $"{Place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public bool AudioIsExisted(string? FullAudioPath)
        {
            return _baseFileService.FileExists(FullAudioPath);
        }

        public async Task<bool> DeleteAudio(string fileName, int Place)
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

        public bool IsValidAudio(string AudioName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(AudioName) && AudioName != PlaceHolder;
        }

        public bool IsValidAudio(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".mp3", ".wav", ".ogg", ".m4a", ".aac" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        private string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place)!;
        }
    }
}
