using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Commands.CreateDoctorWithAvailability
{
    public class CreateDoctorWithAvailabilityCommandHandler : IRequestHandler<CreateDoctorWithAvailabilityCommand, DoctorDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateDoctorWithAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<DoctorDto> Handle(CreateDoctorWithAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.GetByIdAsync(request.ClinicId);
            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new NotFoundException(LocalizationKeys.AuthMessages.UserNotFound.Value);

            var specializationExists = await _unitOfWork.SpecializationRepository.ExistsAsync(s => s.Id == request.SpecializationId, cancellationToken);
            if (!specializationExists)
                throw new NotFoundException(LocalizationKeys.SpecializationMessages.NotFound.Value);

            var existingDoctor = await _unitOfWork.DoctorRepository.GetAllAsync(d => d.UserId == request.UserId && d.ClinicId == request.ClinicId)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingDoctor != null)
                throw new BadRequestException(LocalizationKeys.DoctorMessages.AlreadyExistsInClinic.Value);

            var doctor = new Doctor(
                request.UserId,
                request.ClinicId,
                request.SpecializationId,
                request.Bio,
                request.YearsOfExperience);

            await _unitOfWork.DoctorRepository.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            foreach (var av in request.Availabilities)
            {
                var availability = new DoctorAvailability(
                    doctor.Id,
                    request.ClinicId,
                    av.DayOfWeek,
                    av.StartTime,
                    av.EndTime,
                    av.SlotDurationMinutes);

                await _unitOfWork.DoctorAvailabilityRepository.AddAsync(availability);
            }

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<DoctorDto>(doctor);
        }
    }
}
