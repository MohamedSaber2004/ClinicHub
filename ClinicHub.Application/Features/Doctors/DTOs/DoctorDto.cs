namespace ClinicHub.Application.Features.Doctors.DTOs
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public Guid ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public Guid SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DoctorAvailabilityDto> Availabilities { get; set; } = new();
    }
}
