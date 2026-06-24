using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class BookingConfiguration : BaseEntity<Guid>
    {
        public Guid ClinicId { get; private set; }
        public Clinic Clinic { get; private set; } = null!;

        public decimal ConsultationFee { get; private set; }
        public string Currency { get; private set; } = "EGP";
        public int SlotDurationMinutes { get; private set; } = 30;
        public int MaxAdvanceBookingDays { get; private set; } = 30;
        public int ReservationTtlMinutes { get; private set; } = 10;
        private BookingConfiguration() { }

        public BookingConfiguration(
            Guid clinicId,
            decimal consultationFee,
            string? currency,
            int slotDurationMinutes,
            int maxAdvanceBookingDays,
            int reservationTtlMinutes)
        {
            ClinicId = clinicId;
            ConsultationFee = consultationFee;
            Currency = currency ?? "EGP";
            SlotDurationMinutes = slotDurationMinutes > 0 ? slotDurationMinutes : 30;
            MaxAdvanceBookingDays = maxAdvanceBookingDays > 0 ? maxAdvanceBookingDays : 30;
            ReservationTtlMinutes = reservationTtlMinutes > 0 ? reservationTtlMinutes : 10;
        }

        public void Update(
            decimal consultationFee,
            string? currency,
            int slotDurationMinutes,
            int maxAdvanceBookingDays,
            int reservationTtlMinutes)
        {
            if (consultationFee < 0)
                throw new ArgumentException("Consultation fee must be non-negative", nameof(consultationFee));
            if (slotDurationMinutes <= 0)
                throw new ArgumentException("Slot duration must be greater than 0", nameof(slotDurationMinutes));
            if (maxAdvanceBookingDays <= 0)
                throw new ArgumentException("Max advance booking days must be greater than 0", nameof(maxAdvanceBookingDays));
            if (reservationTtlMinutes <= 0)
                throw new ArgumentException("Reservation TTL must be greater than 0", nameof(reservationTtlMinutes));

            ConsultationFee = consultationFee;
            Currency = currency ?? "EGP";
            SlotDurationMinutes = slotDurationMinutes;
            MaxAdvanceBookingDays = maxAdvanceBookingDays;
            ReservationTtlMinutes = reservationTtlMinutes;
        }
    }
}
