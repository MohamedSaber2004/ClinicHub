using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.StaffDashboard.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.StaffDashboard.Queries.GetStaffDoctors
{
    public class GetStaffDoctorsQueryHandler : IRequestHandler<GetStaffDoctorsQuery, List<DoctorBriefDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStaffDoctorsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<DoctorBriefDto>> Handle(GetStaffDoctorsQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new List<DoctorBriefDto>();

            var doctors = await _unitOfWork.DoctorRepository
                .GetAllWithIncluding(
                    d => d.ClinicId == clinicId && !d.IsDeleted,
                    d => d.User,
                    d => d.Specialization)
                .OrderBy(d => d.User.FullName)
                .ToListAsync(cancellationToken);

            return doctors.Select(d => new DoctorBriefDto
            {
                Id = d.Id,
                Name = "د. " + d.User.FullName,
                Specialty = d.Specialization.ArName ?? d.Specialization.Name
            }).ToList();
        }
    }
}
