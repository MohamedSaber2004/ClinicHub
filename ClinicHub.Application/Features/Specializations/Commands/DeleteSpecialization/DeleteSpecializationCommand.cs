using MediatR;

namespace ClinicHub.Application.Features.Specializations.Commands.DeleteSpecialization
{
    public record DeleteSpecializationCommand(Guid Id) : IRequest<string>;
}
