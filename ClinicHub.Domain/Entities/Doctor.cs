using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Doctor : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;

        public Guid ClinicId { get; private set; }
        public Clinic Clinic { get; private set; } = null!;

        public Guid SpecializationId { get; private set; }
        public Specialization Specialization { get; private set; } = null!;

        public string Bio { get; private set; } = string.Empty;
        public decimal ConsultationFee { get; private set; }
        public int YearsOfExperience { get; private set; }

        public virtual ICollection<DoctorAvailability> Availabilities { get; private set; } = new List<DoctorAvailability>();
        public virtual ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();

        private Doctor() { }

        public Doctor(
            Guid userId,
            Guid clinicId,
            Guid specializationId,
            string bio,
            decimal consultationFee,
            int yearsOfExperience)
        {
            UserId = userId;
            ClinicId = clinicId;
            SpecializationId = specializationId;
            Bio = bio;
            ConsultationFee = consultationFee;
            YearsOfExperience = yearsOfExperience;
        }

        public void Update(string bio, decimal consultationFee, int yearsOfExperience)
        {
            Bio = bio;
            ConsultationFee = consultationFee;
            YearsOfExperience = yearsOfExperience;
        }
    }
}
