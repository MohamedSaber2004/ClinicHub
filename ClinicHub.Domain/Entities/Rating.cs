using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities
{
    public class Rating : BaseEntity<Guid>
    {
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;

        public Guid? DoctorId { get; private set; }
        public Doctor? Doctor { get; private set; }

        public Guid? ClinicId { get; private set; }
        public Clinic? Clinic { get; private set; }

        public int Value { get; private set; }
        public string? Review { get; private set; }

        private Rating() { }

        public Rating(Guid userId, Guid? doctorId, Guid? clinicId, int value, string? review)
        {
            if (doctorId == null && clinicId == null)
                throw new ArgumentException("Either doctorId or clinicId must be provided");
            if (doctorId != null && clinicId != null)
                throw new ArgumentException("Only one of doctorId or clinicId should be provided");
            if (value < 1 || value > 5)
                throw new ArgumentException("Rating value must be between 1 and 5");

            UserId = userId;
            DoctorId = doctorId;
            ClinicId = clinicId;
            Value = value;
            Review = review;
        }
    }
}
