using AutoMapper;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAllAppointmentsWithFilters
{
    public class GetAllAppointmentsWithFiltersQueryHandler : IRequestHandler<GetAllAppointmentsWithFiltersQuery, PagginatedResult<AppointmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllAppointmentsWithFiltersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<AppointmentDto>> Handle(GetAllAppointmentsWithFiltersQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _unitOfWork.AppointmentRepository.GetAppointmentsWithFiltersAsync(
                request.PageNumber,
                request.PageSize,
                request.DoctorId,
                request.ClinicId,
                request.StartDate.HasValue ? request.StartDate.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                request.Status,
                request.PatientName);

            var dtos = _mapper.Map<List<AppointmentDto>>(items);

            return new PagginatedResult<AppointmentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
