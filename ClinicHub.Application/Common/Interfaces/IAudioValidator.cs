using Microsoft.AspNetCore.Http;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IAudioValidator
    {
        public Task<(bool Uploaded, string Result)> UploadAudio(IFormFile file, int Place);
        public bool AudioIsExisted(string? FullAudioPath);
        public Task<bool> DeleteAudio(string fileName, int Place);
        public string GetUniqueFileName(string fileName);
        public bool IsValidAudio(string AudioName, string PlaceHolder);
        public bool IsValidAudio(IFormFile file);
    }
}
