using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IImageValidator
    {
        public Task<(bool Uploaded, string Result)> UploadImage(IFormFile file, int Place);
        public Task<(bool Uploaded, string Result)> UploadMultipleImage(List<IFormFile> file, int Place);
        public bool ImageIsExisted(string? FullImagePath);
        public Task<bool> DeleteImage(string fileName, int Place);
        public string GetUniqueFileName(string fileName);

        public bool IsValidImage(IFormFile file);
        public bool IsValidImage(string ImageName, string PlaceHolder);

        public Task<IFormFile> ConvertImageToFormFile(string imageUrl);

    }
}
