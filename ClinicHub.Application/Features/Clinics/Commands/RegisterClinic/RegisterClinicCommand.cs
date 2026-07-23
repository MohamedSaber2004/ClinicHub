using ClinicHub.Application.Features.Auth.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Commands.RegisterClinic
{
    public class RegisterClinicCommand : IRequest<SignupResult>
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? FcmToken { get; set; }
        public DevicePlatform? DevicePlatform { get; set; }

        public string ClinicName { get; set; } = null!;
        public string? ClinicNameAr { get; set; }
        public string? ClinicDescription { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicPhone { get; set; }
        public string? ClinicEmail { get; set; }
        public Guid SpecializationId { get; set; }
        public string? WorkingHours { get; set; }
        public TimeOnly? WorkingHoursStart { get; set; }
        public TimeOnly? WorkingHoursEnd { get; set; }
        public List<string>? WorkingDays { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string? Logo { get; set; }

        public string? ProfessionalPracticeCardImage { get; set; }
        public string? TaxCardImage { get; set; }
        public string? UnionIdCardImage { get; set; }
        public string? DoctorImage { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
    }
}
