using ClinicHub.Application.Features.Ads.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Ads.Queries.GetClinicAdSettings;

public class GetClinicAdSettingsQuery : IRequest<List<ClinicAdSettingsDto>>
{
}
