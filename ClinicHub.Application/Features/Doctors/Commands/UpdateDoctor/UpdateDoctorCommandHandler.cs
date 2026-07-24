using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, DoctorDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public UpdateDoctorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<DoctorDto> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var doctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(null)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

            if (doctor == null || doctor.ClinicId != clinicId)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            if (request.Bio != null || request.YearsOfExperience.HasValue)
                doctor.Update(
                    request.Bio ?? doctor.Bio,
                    request.YearsOfExperience ?? doctor.YearsOfExperience);

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    doctor.Active();
                else
                    doctor.Deactive();
            }

            _unitOfWork.DoctorRepository.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DoctorDto>(doctor);
        }
    }
}
