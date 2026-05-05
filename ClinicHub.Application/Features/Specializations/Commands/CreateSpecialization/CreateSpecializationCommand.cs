using ClinicHub.Application.Common.Models;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Commands.CreateSpecialization
{
    public class CreateSpecializationCommand : IRequest<string>
    {
        public string Name { get; set; } = null!;
        public string ArName { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }
}
