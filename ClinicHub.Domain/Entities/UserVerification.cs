using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class UserVerification : BaseEntity<Guid>
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public UserType RequestedRole { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public ApplicationUser? ReviewedBy { get; set; }
        public string? Notes { get; set; }
        public string? ProfessionalPracticeCardImage { get; set; }
        public string? TaxCardImage { get; set; }
        public string? UnionIdCardImage { get; set; }
        public string? DoctorImage { get; set; }
        public Guid? SpecializationId { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }

        public static UserVerification Create(Guid userId, UserType requestedRole, string? professionalPracticeCardImage = null, string? taxCardImage = null, string? unionIdCardImage = null, string? doctorImage = null, Guid? specializationId = null, string? bio = null, int? yearsOfExperience = null) => new()
        {
            UserId = userId,
            RequestedRole = requestedRole,
            Status = VerificationStatus.Pending,
            RequestedAt = DateTime.Now,
            ProfessionalPracticeCardImage = professionalPracticeCardImage,
            TaxCardImage = taxCardImage,
            UnionIdCardImage = unionIdCardImage,
            DoctorImage = doctorImage,
            SpecializationId = specializationId,
            Bio = bio,
            YearsOfExperience = yearsOfExperience
        };

        public void UpdateDoctorImage(string? doctorImage) => DoctorImage = doctorImage;

        public void Approve(Guid reviewedByUserId)
        {
            Status = VerificationStatus.Approved;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = DateTime.Now;
        }

        public void Reject(Guid reviewedByUserId, string? notes)
        {
            Status = VerificationStatus.Rejected;
            ReviewedByUserId = reviewedByUserId;
            ReviewedAt = DateTime.Now;
            Notes = notes;
        }
    }
}
