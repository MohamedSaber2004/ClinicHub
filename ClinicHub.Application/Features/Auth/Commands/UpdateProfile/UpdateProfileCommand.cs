using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommand : IRequest<bool>
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string? ProfileImageUrl { get; set; }
    }
}
