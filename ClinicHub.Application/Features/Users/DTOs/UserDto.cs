using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Users.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public bool IsActive { get; set; }
        public IList<UserType> Roles { get; set; } = new List<UserType>();
        public DateTime CreatedAt { get; set; }
        public int TotalVisits { get; set; }
        public double AvgRating { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
