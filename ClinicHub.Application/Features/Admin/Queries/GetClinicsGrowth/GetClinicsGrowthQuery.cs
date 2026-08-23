using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicsGrowth
{
    public record GetClinicsGrowthQuery(string Granularity = "day", DateTime? FromDate = null, DateTime? ToDate = null)
        : IRequest<List<ClinicsGrowthPointDto>>;
}
