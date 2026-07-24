using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Doctors.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorsByClinic
{
    public class GetDoctorsByClinicQuery : IRequest<PagginatedResult<DoctorDto>>
    {
        public Guid ClinicId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SearchTerm { get; set; }
        public Guid? SpecializationId { get; set; }
        public bool? IsActive { get; set; }
    }
}
