using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Clinics.DTOs
{
    public record UpdateClinicDto(
        string? Name,
        string? NameAr,
        string? Description,
        string? ArDescription,
        string? Address,
        string? AddressAr,
        string? Phone,
        string Email,
        string? Website,
        string? Logo,
        string? WorkingHours,
        Guid SpecializationId,
        TimeOnly? WorkingHoursStart = null,
        TimeOnly? WorkingHoursEnd = null,
        List<DayOfWeek>? WorkingDays = null);

    public class ClinicSettingsDto
    {
        public string Name { get; set; } = null!;
        public string? ResponsibleDoctor { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
        public string? Location { get; set; }
        public Guid SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public string? SpecializationNameAr { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsActive { get; set; }
        public decimal ConsultationFee { get; set; }
        public string Currency { get; set; } = "EGP";
        public int MaxAdvanceBookingDays { get; set; } = 30;
        public int ReservationTtlMinutes { get; set; } = 10;
        public int SlotDurationMinutes { get; set; } = 30;
    }

    public class ClinicManagementDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? NameAr { get; set; }
        public string? Description { get; set; }
        public string? ArDescription { get; set; }
        public string? Address { get; set; }
        public string? AddressAr { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string? WorkingHours { get; set; }
        public List<WorkingDayDto>? WorkingDays { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public bool IsRegistered { get; set; }
        public ClinicStatus Status { get; set; }
        public bool IsActive { get; set; }
        public Guid SpecializationId { get; set; }
        public string? SpecializationName { get; set; }
        public string? SpecializationNameAr { get; set; }
        public double? Rating { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? ClinicAdminId { get; set; }
        public string? OwnerName { get; set; }
        public string? OwnerEmail { get; set; }
        public string? OwnerPhone { get; set; }
        public SubscriptionStatus? SubscriptionStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? UpdatedBy { get; set; }
        public List<ClinicDoctorDto>? Doctors { get; set; }
    }
}
