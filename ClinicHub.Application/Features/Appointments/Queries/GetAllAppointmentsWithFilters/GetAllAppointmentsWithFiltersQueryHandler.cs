using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Appointments.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAllAppointmentsWithFilters
{
    public class GetAllAppointmentsWithFiltersQueryHandler : IRequestHandler<GetAllAppointmentsWithFiltersQuery, PagginatedResult<AppointmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetAllAppointmentsWithFiltersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PagginatedResult<AppointmentDto>> Handle(GetAllAppointmentsWithFiltersQuery request, CancellationToken cancellationToken)
        {
            // Staff see the clinic queue (including Pending requests awaiting their
            // decision — clinic scoping comes from the EF global clinic filter).
            // Patients only ever see their own bookings.
            Guid? bookedByUserId = IsStaffQueueViewer(_currentUserService.UserTypes)
                ? null
                : _currentUserService.UserId;

            var (items, totalCount) = await _unitOfWork.AppointmentRepository.GetAppointmentsWithFiltersAsync(
                request.PageNumber,
                request.PageSize,
                request.DoctorId,
                request.ClinicId,
                request.StartDate.HasValue ? request.StartDate.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                request.EndDate.HasValue ? request.EndDate.Value.ToString("dd/MM/yyyy hh:mm tt") : null,
                request.Status,
                request.PatientName,
                bookedByUserId);

            var dtos = _mapper.Map<List<AppointmentDto>>(items);

            return new PagginatedResult<AppointmentDto>(dtos, totalCount, request.PageNumber, request.PageSize);
        }

        private static bool IsStaffQueueViewer(int? userTypes) =>
            userTypes.HasValue
            && (userTypes.Value & (int)(UserType.ClinicOwner | UserType.Staff | UserType.SuperAdmin)) != 0;
    }
}
