using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class PlatformSetting : BaseEntity<Guid>
    {
        public decimal AppointmentFeePercent { get; private set; }

        /// <summary>Lowest clinic balance that can be paid out; maintained by the admin.</summary>
        public decimal MinimumPayoutAmount { get; private set; } = 120m;

        private PlatformSetting() { }

        public PlatformSetting(decimal appointmentFeePercent, decimal minimumPayoutAmount = 120m)
        {
            ApplyPercent(appointmentFeePercent);
            ApplyMinimumPayout(minimumPayoutAmount);
            MarkAsCreated("system");
        }

        public void UpdateAppointmentFeePercent(decimal percent, string updatedBy)
        {
            ApplyPercent(percent);
            MarkAsUpdated(updatedBy);
        }

        public void UpdateMinimumPayoutAmount(decimal minimumPayoutAmount, string updatedBy)
        {
            ApplyMinimumPayout(minimumPayoutAmount);
            MarkAsUpdated(updatedBy);
        }

        private void ApplyPercent(decimal percent)
        {
            if (percent < 0 || percent > 100)
                throw new ArgumentOutOfRangeException(nameof(percent), "Fee percentage must be between 0 and 100.");

            AppointmentFeePercent = Math.Round(percent, 2);
        }

        private void ApplyMinimumPayout(decimal minimumPayoutAmount)
        {
            if (minimumPayoutAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(minimumPayoutAmount), "Minimum payout amount must be greater than zero.");

            MinimumPayoutAmount = Math.Round(minimumPayoutAmount, 2);
        }
    }
}
