using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Services;
using ClinicHub.Application.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Infrastructure.Services.Attachements
{
    public class ImageValidator : IImageValidator
    {
        private readonly IBaseFileService _baseFileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStringLocalizer<Messages> _localizer;

        public ImageValidator(IBaseFileService baseFileService, IHttpClientFactory httpClientFactory, IStringLocalizer<Messages> localizer)
        {
            _baseFileService = baseFileService;
            _httpClientFactory = httpClientFactory;
            _localizer = localizer;
        }

        public async Task<(bool Uploaded, string Result)> UploadImage(IFormFile file, int Place)
        {
            if (!IsValidImage(file))
                return (false, _localizer["Attachments:InvalidFormat"]);

            var (uploaded, result) = await _baseFileService.UploadFileAsync(file, GetFolderPath(Place));
            if (uploaded)
            {
                return (true, $"{Place}_{Path.GetFileName(result)}");
            }
            return (false, result);
        }

        public async Task<(bool Uploaded, string Result)> UploadMultipleImage(List<IFormFile> files, int Place)
        {
            if (files == null || !files.Any())
                return (false, _localizer["Attachments:NoMediaProvided"]);

            var results = new List<string>();
            foreach (var file in files)
            {
                var (uploaded, result) = await UploadImage(file, Place);
                if (uploaded)
                {
                    results.Add(result);
                }
            }

            if (!results.Any())
                return (false, _localizer["Attachments:UploadFailed"]);

            return (true, string.Join(",", results));
        }

        public bool ImageIsExisted(string? FullImagePath)
        {
            return _baseFileService.FileExists(FullImagePath);
        }

        public async Task<bool> DeleteImage(string fileName, int Place)
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

        public bool IsValidImage(IFormFile file)
        {
            if (file == null) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowedExtensions.Contains(extension);
        }

        public bool IsValidImage(string ImageName, string PlaceHolder)
        {
            return !string.IsNullOrEmpty(ImageName) && ImageName != PlaceHolder;
        }

        public async Task<IFormFile> ConvertImageToFormFile(string imageUrl)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(imageUrl);
            if (!response.IsSuccessStatusCode)
                return null!;

            var content = await response.Content.ReadAsByteArrayAsync();
            var stream = new MemoryStream(content);

            var fileName = Path.GetFileName(imageUrl);
            if (string.IsNullOrEmpty(fileName))
                fileName = "downloaded_image.jpg";

            return new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = response.Content.Headers.ContentType?.ToString()!
            };
        }

        private string GetFolderPath(int place)
        {
            return UploadPaths.GetPath(place)!;
        }
    }
}
