using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicsLookup
{
    public class GetClinicsLookupQuery : IRequest<List<ClinicLookupDto>>
    {
    }
}
