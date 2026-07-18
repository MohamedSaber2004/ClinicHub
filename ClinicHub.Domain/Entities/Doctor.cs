using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;

namespace ClinicHub.Domain.Entities
{
    public class Doctor : BaseEntity<Guid>, IClinicScopedEntity
    {
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;

        public Guid? ClinicId { get; private set; }
        Guid? IClinicScopedEntity.ClinicId => ClinicId;
        public Clinic? Clinic { get; private set; }

        public Guid SpecializationId { get; private set; }
        public Specialization Specialization { get; private set; } = null!;

        public string Bio { get; private set; } = string.Empty;
        public int YearsOfExperience { get; private set; }

        public virtual ICollection<DoctorAvailability> Availabilities { get; private set; } = new List<DoctorAvailability>();
        public virtual ICollection<Appointment> Appointments { get; private set; } = new List<Appointment>();

        private Doctor() { }

        public Doctor(
            Guid userId,
            Guid specializationId,
            string bio,
            int yearsOfExperience) : this(userId, null, specializationId, bio, yearsOfExperience)
        {
        }

        public Doctor(
            Guid userId,
            Guid? clinicId,
            Guid specializationId,
            string bio,
            int yearsOfExperience)
        {
            UserId = userId;
            ClinicId = clinicId;
            SpecializationId = specializationId;
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
        }

        public void AssignToClinic(Guid clinicId)
        {
            ClinicId = clinicId;
        }

        public void Update(string bio, int yearsOfExperience)
        {
            Bio = bio;
            YearsOfExperience = yearsOfExperience;
        }
    }
}
