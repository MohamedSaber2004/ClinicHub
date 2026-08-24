using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class PlatformSetting : BaseEntity<Guid>
    {
        public decimal AppointmentFeePercent { get; private set; }

        private PlatformSetting() { }

        public PlatformSetting(decimal appointmentFeePercent)
        {
            ApplyPercent(appointmentFeePercent);
            MarkAsCreated("system");
        }

        public void UpdateAppointmentFeePercent(decimal percent, string updatedBy)
        {
            ApplyPercent(percent);
            MarkAsUpdated(updatedBy);
        }

        private void ApplyPercent(decimal percent)
        {
            if (percent < 0 || percent > 100)
                throw new ArgumentOutOfRangeException(nameof(percent), "Fee percentage must be between 0 and 100.");

            AppointmentFeePercent = Math.Round(percent, 2);
        }
    }
}
