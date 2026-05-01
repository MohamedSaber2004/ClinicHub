using ClinicHub.Application.Common.Options;
using Google.Apis.Http;
using Microsoft.Extensions.Configuration;

namespace ClinicHub.Application.Common.Services
{
    public class UploadPaths
    {
        private static UploadPathsOptions? Options;

        public static void Configure(IConfiguration configuration)
        {
            Options = configuration.GetSection("UploadPaths").Get<UploadPathsOptions>();
        }

        public static string? DefaultUserImage => Options?.DefaultUserImage;
        public static string? UserImages => Options?.UserImages;
        public static string? PostImages => Options?.PostImages;
        public static string? PostVideos => Options?.PostVideos;
        public static string? PostDocuments => Options?.PostDocuments;
        public static string? ClinicImages => Options?.ClinicImages;
        public static string? ClinicDocuments => Options?.ClinicDocuments;
        public static string? DoctorImages => Options?.DoctorImages;
        public static string? DoctorDocuments => Options?.DoctorDocuments;
        public static string? MessageImages => Options?.MessageImages;
        public static string? MessageVideos => Options?.MessageVideos;
        public static string? MessageDocuments => Options?.MessageDocuments;
        public static string? MessageAudio => Options?.MessageAudio;

        public static string? GetPath(int place)
        {
            return place switch
            {
                0 => DefaultUserImage,
                1 => UserImages,
                2 => PostImages,
                3 => PostVideos,
                4 => PostDocuments,
                5 => ClinicImages,
                6 => ClinicDocuments,
                7 => DoctorImages,
                8 => DoctorDocuments,
                9 => MessageImages,
                10 => MessageVideos,
                11 => MessageDocuments,
                12 => MessageAudio,
                _ => string.Empty
            };
        }

        public static IEnumerable<string> GetAllPaths()
        {
            if (Options is null) yield break;
            yield return Options.DefaultUserImage;
            yield return Options.UserImages;
            yield return Options.PostImages;
            yield return Options.PostVideos;
            yield return Options.PostDocuments;
            yield return Options.ClinicImages;
            yield return Options.ClinicDocuments;
            yield return Options.DoctorImages;
            yield return Options.DoctorDocuments;
            yield return Options.MessageImages;
            yield return Options.MessageVideos;
            yield return Options.MessageDocuments;
            yield return Options.MessageAudio;
        }
    }
}
