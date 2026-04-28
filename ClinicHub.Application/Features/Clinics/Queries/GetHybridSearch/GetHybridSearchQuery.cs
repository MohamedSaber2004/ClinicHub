using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch
{
    public class GetHybridSearchQuery : IRequest<PagginatedResult<ClinicDto>>
    {
        public string? SearchText { get; set; }
        public Guid? SpecializationId { get; set; }
        public double? UserLat { get; set; }
        public double? UserLng { get; set; }
        public bool IsNearest { get; set; }
        public double RadiusInKm { get; set; } = 5;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
