namespace ClinicHub.Application.Features.Specializations.DTOs
{
    public class SpecializationLookupDto
    {
        public Guid Id { get; set; }
        public string ArName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsFamous { get; set; }
    }
}
