using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctorSchedule
{
    public class GetStaffDoctorScheduleQueryHandler : IRequestHandler<GetStaffDoctorScheduleQuery, List<AvailabilityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetStaffDoctorScheduleQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<List<AvailabilityDto>> Handle(GetStaffDoctorScheduleQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;

            var availabilities = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == request.DoctorId && a.ClinicId == clinicId && !a.IsDeleted)
                .OrderBy(a => a.DayOfWeek)
                    .ThenBy(a => a.StartTime)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<AvailabilityDto>>(availabilities);
        }
    }
}
