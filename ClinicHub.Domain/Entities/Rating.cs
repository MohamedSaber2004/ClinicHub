using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities
{
    public class Rating : BaseEntity<Guid>, IClinicScopedEntity
    {
        public RatingType Type { get; private set; }
        public Guid UserId { get; private set; }
        public ApplicationUser User { get; private set; } = null!;

        public Guid? DoctorId { get; private set; }
        public Doctor? Doctor { get; private set; }

        public Guid? ClinicId { get; private set; }
        public Clinic? Clinic { get; private set; }

        public int Value { get; private set; }
        public string? Review { get; private set; }

        private Rating() { }

        public Rating(Guid userId, RatingType type, Guid? doctorId, Guid? clinicId, int value, string? review)
        {
            switch (type)
            {
                case RatingType.Doctor:
                    if (doctorId == null)
                        throw new ArgumentException("doctorId is required for doctor ratings");
                    break;
                case RatingType.Clinic:
                case RatingType.PlaceCleanliness:
                case RatingType.Reception:
                    if (clinicId == null)
                        throw new ArgumentException("clinicId is required for clinic ratings");
                    break;
                default:
                    throw new ArgumentException("Invalid rating type");
            }

            if (doctorId != null && clinicId != null)
                throw new ArgumentException("Only one of doctorId or clinicId should be provided");
            if (value < 1 || value > 5)
                throw new ArgumentException("Rating value must be between 1 and 5");

            Type = type;
            UserId = userId;
            DoctorId = doctorId;
            ClinicId = clinicId;
            Value = value;
            Review = review;
        }
    }
}
