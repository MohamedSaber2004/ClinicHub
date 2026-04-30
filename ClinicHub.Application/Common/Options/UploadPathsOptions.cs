namespace ClinicHub.Application.Common.Options
{
    public class UploadPathsOptions
    {
        public string DefaultUserImage { get; set; } = null!;
        public string UserImages { get; set; } = null!;
        public string PostImages { get; set; } = null!;
        public string PostVideos { get; set; } = null!;
        public string PostDocuments { get; set; } = null!;
        public string ClinicImages { get; set; } = null!;
        public string ClinicDocuments { get; set; } = null!;
        public string DoctorImages { get; set; } = null!;
        public string DoctorDocuments { get; set; } = null!;
        public string MessageImages { get; set; } = null!;
        public string MessageVideos { get; set; } = null!;
        public string MessageDocuments { get; set; } = null!;
        public string MessageAudio { get; set; } = null!;
    }
}
