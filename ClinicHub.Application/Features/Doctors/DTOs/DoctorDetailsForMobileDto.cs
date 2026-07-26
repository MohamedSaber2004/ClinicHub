namespace ClinicHub.Application.Features.Doctors.DTOs
{
    public class DoctorDetailsForMobileDto
    {
        // Basic info
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public bool IsFreelance { get; set; }

        // Clinic & Specialization
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicNameAr { get; set; }
        public Guid SpecializationId { get; set; }
        public string? SpecializationName { get; set; }

        // Rating summary
        public double? AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public List<DoctorRatingSummaryDto> RecentRatings { get; set; } = [];

        // Availability schedule
        public List<DoctorAvailabilityDto> Availabilities { get; set; } = [];
    }

    public class DoctorAvailabilityDto
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
    }

    public class DoctorRatingSummaryDto
    {
        public Guid Id { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string? ReviewerProfilePictureUrl { get; set; }
        public int Value { get; set; }
        public string? Review { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
