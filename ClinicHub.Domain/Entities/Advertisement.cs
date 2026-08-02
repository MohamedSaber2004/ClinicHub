using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Advertisement : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid? ClinicId { get; set; }
        public Clinic? Clinic { get; set; }
        public Guid? AdPackageId { get; set; }
        public AdPackage? AdPackage { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string? TargetUrl { get; set; }
        public int DurationDays { get; set; }
        public decimal AmountPaid { get; set; }
        public string Currency { get; set; } = "EGP";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AdvertisementStatus Status { get; set; } = AdvertisementStatus.PendingPayment;
        public Guid? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public void Activate(DateTime startDate, int durationDays)
        {
            StartDate = startDate;
            EndDate = startDate.AddDays(durationDays);
            Status = AdvertisementStatus.Active;
        }

        public void Deactivate()
        {
            Status = AdvertisementStatus.Deactivated;
        }
    }
}
