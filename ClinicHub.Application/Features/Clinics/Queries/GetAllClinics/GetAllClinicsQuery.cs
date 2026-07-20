using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Clinics.Queries.GetAllClinics
{
    public class GetAllClinicsQuery : IRequest<List<ClinicManagementDto>>
    {
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
