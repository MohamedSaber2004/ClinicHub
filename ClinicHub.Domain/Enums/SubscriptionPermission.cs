namespace ClinicHub.Domain.Enums
{
    [Flags]
    public enum SubscriptionPermission
    {
        None = 0,
        ManageAppointments = 1,
        PatientRecords = 2,
        BasicReports = 4,
        AdvancedReports = 8,
        [Obsolete("Ads is now independent from subscription plans. Do not assign to new plans. Kept for backward compatibility with old Enterprise subscriptions.")]
        MarketingTools = 16,
        ManageStaff = 64,
        ManageDoctors = 128,
        OnlineBooking = 256,
        All = ~0
    }
}
