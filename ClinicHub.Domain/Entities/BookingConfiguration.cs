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
        public int MaxFutureDays { get; private set; } = 30;
        public int ReservationTtlMinutes { get; private set; } = 10;
        public string PaymentMethods { get; private set; } = "credit_card,cash";
        public bool AllowOnlineBooking { get; private set; } = true;
        public bool RequirePayment { get; private set; } = true;

        private BookingConfiguration() { }

        public BookingConfiguration(
            Guid clinicId,
            decimal consultationFee,
            string? currency,
            int slotDurationMinutes,
            int maxFutureDays,
            int reservationTtlMinutes,
            string? paymentMethods,
            bool allowOnlineBooking,
            bool requirePayment)
        {
            ClinicId = clinicId;
            ConsultationFee = consultationFee;
            Currency = currency ?? "EGP";
            SlotDurationMinutes = slotDurationMinutes > 0 ? slotDurationMinutes : 30;
            MaxFutureDays = maxFutureDays > 0 ? maxFutureDays : 30;
            ReservationTtlMinutes = reservationTtlMinutes > 0 ? reservationTtlMinutes : 10;
            PaymentMethods = paymentMethods ?? "credit_card,cash";
            AllowOnlineBooking = allowOnlineBooking;
            RequirePayment = requirePayment;
        }

        public void Update(
            decimal consultationFee,
            string? currency,
            int slotDurationMinutes,
            int maxFutureDays,
            int reservationTtlMinutes,
            string? paymentMethods,
            bool allowOnlineBooking,
            bool requirePayment)
        {
            ConsultationFee = consultationFee;
            Currency = currency ?? "EGP";
            SlotDurationMinutes = slotDurationMinutes;
            MaxFutureDays = maxFutureDays;
            ReservationTtlMinutes = reservationTtlMinutes;
            PaymentMethods = paymentMethods ?? "credit_card,cash";
            AllowOnlineBooking = allowOnlineBooking;
            RequirePayment = requirePayment;
        }
    }
}
