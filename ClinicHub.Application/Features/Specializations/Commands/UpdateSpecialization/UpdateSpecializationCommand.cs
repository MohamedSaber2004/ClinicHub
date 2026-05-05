using MediatR;

namespace ClinicHub.Application.Features.Specializations.Commands.UpdateSpecialization
{
    public class UpdateSpecializationCommand : IRequest<string>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string ArName { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }
}
