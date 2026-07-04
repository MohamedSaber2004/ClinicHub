using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;

namespace ClinicHub.Domain.Entities
{
    public class BookingConfiguration : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid ClinicId { get; private set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic Clinic { get; private set; } = null!;

        public decimal ConsultationFee { get; private set; }
        public string Currency { get; private set; } = "EGP";
        public int MaxAdvanceBookingDays { get; private set; } = 30;
        public int ReservationTtlMinutes { get; private set; } = 10;
        private BookingConfiguration() { }

        public BookingConfiguration(
            Guid clinicId,
            decimal consultationFee,
            string? currency,
            int maxAdvanceBookingDays,
            int reservationTtlMinutes)
        {
            ClinicId = clinicId;
            ConsultationFee = consultationFee;
            Currency = currency ?? "EGP";
            MaxAdvanceBookingDays = maxAdvanceBookingDays > 0 ? maxAdvanceBookingDays : 30;
            ReservationTtlMinutes = reservationTtlMinutes > 0 ? reservationTtlMinutes : 10;
        }

        public void Update(
            decimal consultationFee,
            string? currency,
            int maxAdvanceBookingDays,
            int reservationTtlMinutes)
        {
            if (consultationFee < 0)
                throw new ArgumentException("Consultation fee must be non-negative", nameof(consultationFee));
            if (maxAdvanceBookingDays <= 0)
                throw new ArgumentException("Max advance booking days must be greater than 0", nameof(maxAdvanceBookingDays));
            if (reservationTtlMinutes <= 0)
                throw new ArgumentException("Reservation TTL must be greater than 0", nameof(reservationTtlMinutes));

            ConsultationFee = consultationFee;
            Currency = currency ?? "EGP";
            MaxAdvanceBookingDays = maxAdvanceBookingDays;
            ReservationTtlMinutes = reservationTtlMinutes;
        }
    }
}
