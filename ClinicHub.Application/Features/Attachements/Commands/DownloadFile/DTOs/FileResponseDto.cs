namespace ClinicHub.Application.Features.Attachements.Commands.DownloadFile.DTOs
{
    public class FileResponseDto
    {
        public string FilePath { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string ContentType { get; set; } = "application/octet-stream";
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
