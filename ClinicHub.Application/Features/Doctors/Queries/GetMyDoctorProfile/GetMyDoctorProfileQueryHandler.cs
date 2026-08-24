using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Queries.GetMyDoctorProfile
{
    public class GetMyDoctorProfileQueryHandler : IRequestHandler<GetMyDoctorProfileQuery, DoctorDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyDoctorProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<DoctorDto> Handle(GetMyDoctorProfileQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.UserId == _currentUserService.UserId)
                .Include(d => d.User)
                .Include(d => d.Clinic)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(cancellationToken);

            if (doctor == null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            return _mapper.Map<DoctorDto>(doctor);
        }
    }
}
