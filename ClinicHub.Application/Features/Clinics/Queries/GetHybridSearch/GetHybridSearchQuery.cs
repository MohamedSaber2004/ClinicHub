using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch
{
    public class GetHybridSearchQuery : IRequest<List<ClinicDto>>
    {
        public string? SearchText { get; set; }
        public string? SpecializationId { get; set; }
        public double? UserLat { get; set; }
        public double? UserLng { get; set; }
        public bool IsNearest { get; set; }
        public double RadiusInKm { get; set; } = 5;
    }
}
