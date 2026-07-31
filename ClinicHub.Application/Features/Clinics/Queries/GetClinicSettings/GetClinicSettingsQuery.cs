using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicSettings
{
    public record GetClinicSettingsQuery : IRequest<ClinicSettingsDto>;
}
