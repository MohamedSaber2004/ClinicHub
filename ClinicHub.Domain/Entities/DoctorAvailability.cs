using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class DoctorAvailability : BaseEntity<Guid>
    {
        public Guid DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;

        public DayOfWeek DayOfWeek { get; private set; }

        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public int SlotDurationMinutes { get; private set; } = 30;

        private DoctorAvailability() { }

        public DoctorAvailability(
            Guid doctorId,
            DayOfWeek dayOfWeek,
            TimeSpan startTime,
            TimeSpan endTime,
            int slotDurationMinutes = 30)
        {
            DoctorId = doctorId;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            SlotDurationMinutes = slotDurationMinutes;
        }

        public void Update(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes)
        {
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            SlotDurationMinutes = slotDurationMinutes;
        }
    }
}
