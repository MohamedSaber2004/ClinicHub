using ClinicHub.Infrastructure.Services.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Maps.Queries.GetRoute
{
    public class GetRouteQueryHandler : IRequestHandler<GetRouteQuery, RouteDto?>
    {
        private readonly IMapService _mapService;

        public GetRouteQueryHandler(IMapService mapService)
        {
            _mapService = mapService;
        }

        public async Task<RouteDto?> Handle(GetRouteQuery request, CancellationToken cancellationToken)
        {
            return await _mapService.GetRouteAsync(
                request.StartLat, 
                request.StartLng, 
                request.EndLat, 
                request.EndLng, 
                cancellationToken);
        }
    }
}
