using System.Text.Json.Serialization;

namespace ClinicHub.Application.Features.Clinics.DTOs
{
    public class ClinicDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public string? Address { get; set; }
        public string? AddressAr { get; set; }
        public string? Phone { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public bool IsRegistered { get; set; }
        public string? SpecializationName { get; set; }
        public string? SpecializationNameAr { get; set; }
        public double Distance { get; set; }
        public string? Website { get; set; }
        public List<WorkingDayDto>? WorkingDays { get; set; }

        public double? Rating { get; set; }
        public int ReviewsCount { get; set; }
        public Dictionary<string, string>? OperatingHours { get; set; }
        public bool? IsOpen { get; set; }

        // 0 means "free"; the key is omitted only when the clinic has no booking configuration.
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ConsultationFee { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ExaminationFee { get; set; }

        public string Currency { get; set; } = "EGP";
    }
}
