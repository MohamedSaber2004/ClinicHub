namespace ClinicHub.Application.Common
{
    /// <summary>
    /// Computes appointment pricing: the platform fee is a percentage of the
    /// clinic's consultation fee and is added on top of it. The clinic keeps
    /// its full consultation fee; the patient pays one combined total.
    /// </summary>
    public static class AppointmentPricingCalculator
    {
        public static decimal CalculatePlatformFee(decimal consultationFee, decimal percent)
            => percent <= 0
                ? 0m
                : Math.Round(consultationFee * percent / 100m, 2, MidpointRounding.AwayFromZero);

        public static decimal CalculateTotal(decimal consultationFee, decimal percent)
            => consultationFee + CalculatePlatformFee(consultationFee, percent);
    }
}
