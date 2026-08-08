using ClinicHub.Domain.Common;
using ClinicHub.Domain.Enums;
using NetTopologySuite.Geometries;

namespace ClinicHub.Domain.Entities
{
    public class Clinic : BaseEntity<Guid>
    {
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
        public string? ResponsibleDoctor { get; set; }
        public string? ManagerName { get; set; }
        public string? WorkingHours { get; set; }
        public TimeOnly? WorkingHoursStart { get; set; }
        public TimeOnly? WorkingHoursEnd { get; set; }
        public string? WorkingDays { get; set; }

        public Point Location { get; set; } = null!;
        public bool IsRegistered { get; set; }
        public bool IsSetupComplete { get; set; }

        public ClinicStatus Status { get; set; } = ClinicStatus.PendingApproval;

        public Guid SpecializationId { get; set; }
        public Specialization Specialization { get; set; } = null!;
        public double? Rating { get; set; }
        public string? ImageUrl { get; set; }

        public Guid? ClinicAdminId { get; set; }
        public ApplicationUser? ClinicAdmin { get; set; }

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        public void UpdateStatus(ClinicStatus status, string updatedBy)
        {
            Status = status;
            if (status == ClinicStatus.Active) Active(); else Deactive();
            MarkAsUpdated(updatedBy);
        }

        public void UpdateDetails(string name, string? nameAr, string? description, string? arDescription, string? address, string? addressAr,
            string? phone, string? email, string? website, string? logo, string? workingHours, Guid specializationId, string updatedBy,
            TimeOnly? workingHoursStart = null, TimeOnly? workingHoursEnd = null, string? workingDays = null, Point? locationPoint = null)
        {
            Name = name;
            NameAr = nameAr;
            Description = description;
            ArDescription = arDescription;
            Address = address;
            AddressAr = addressAr;
            Phone = phone;
            Email = email;
            Website = website;
            Logo = logo;
            WorkingHours = workingHours;
            WorkingHoursStart = workingHoursStart;
            WorkingHoursEnd = workingHoursEnd;
            WorkingDays = workingDays;
            SpecializationId = specializationId;
            if (locationPoint != null)
            {
                Location = locationPoint;
            }
            MarkAsUpdated(updatedBy);
        }

        public void UpdateSettings(string name, string? responsibleDoctor, string? description, string? phone,
            string? managerName, string? location, Guid specializationId, Point? locationPoint, bool isActive, string updatedBy)
        {
            Name = name;
            ResponsibleDoctor = responsibleDoctor;
            Description = description;
            Phone = phone;
            ManagerName = managerName;
            Address = location;
            AddressAr = location;
            SpecializationId = specializationId;
            if (locationPoint != null)
            {
                Location = locationPoint;
            }
            SetActiveState(isActive, updatedBy);
        }
    }
}
