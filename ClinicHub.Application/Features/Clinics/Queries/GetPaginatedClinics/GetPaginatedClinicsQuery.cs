using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetPaginatedClinics
{
    public class GetPaginatedClinicsQuery : IRequest<PagginatedResult<ClinicManagementDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public bool SortAscending { get; set; } = true;
        public string? SearchTerm { get; set; }
        public ClinicStatus? Status { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
        public SubscriptionStatus? SubscriptionStatus { get; set; }
    }
}

