namespace ClinicHub.Application.Features.Doctors.DTOs
{
    public class CreateDoctorDto
    {
        public Guid ClinicId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpecializationId { get; set; }
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
    }
}
