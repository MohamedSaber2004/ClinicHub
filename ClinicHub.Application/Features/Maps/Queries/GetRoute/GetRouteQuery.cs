using ClinicHub.Infrastructure.Services.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Maps.Queries.GetRoute
{
    public class GetRouteQuery : IRequest<RouteDto?>
    {
        public double StartLat { get; set; }
        public double StartLng { get; set; }
        public double EndLat { get; set; }
        public double EndLng { get; set; }
    }
}
