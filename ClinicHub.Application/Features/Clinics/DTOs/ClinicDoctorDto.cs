namespace ClinicHub.Application.Features.Clinics.DTOs
{
    public class ClinicDoctorDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Image { get; set; }
        public string SpecializationArName { get; set; } = null!;
        public string SpecializationEnName { get; set; } = null!;
        public string Bio { get; set; } = null!;
        public int YearsOfExperience { get; set; }
    }
}
