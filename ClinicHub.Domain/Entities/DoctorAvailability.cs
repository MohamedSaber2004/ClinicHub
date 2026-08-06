using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;

namespace ClinicHub.Domain.Entities
{
    public class DoctorAvailability : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;

        public Guid ClinicId { get; private set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic Clinic { get; private set; } = null!;

        public DayOfWeek DayOfWeek { get; private set; }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public int SlotDurationMinutes { get; private set; } = 30;

        private DoctorAvailability() { }

        public DoctorAvailability(
            Guid doctorId,
            Guid clinicId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            int slotDurationMinutes = 30)
        {
            if (slotDurationMinutes <= 0)
                throw new ArgumentException("Slot duration must be greater than 0", nameof(slotDurationMinutes));

            DoctorId = doctorId;
            ClinicId = clinicId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            SlotDurationMinutes = slotDurationMinutes;
        }

        public void Update(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
        {
            if (slotDurationMinutes <= 0)
                throw new ArgumentException("Slot duration must be greater than 0", nameof(slotDurationMinutes));

            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            SlotDurationMinutes = slotDurationMinutes;
        }
    }
}
