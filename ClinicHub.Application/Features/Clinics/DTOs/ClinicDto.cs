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
        public double Distance { get; set; }
    }
}
