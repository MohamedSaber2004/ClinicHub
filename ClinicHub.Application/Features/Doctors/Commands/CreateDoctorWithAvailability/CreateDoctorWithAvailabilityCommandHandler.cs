using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
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

            var specializationExists = await _unitOfWork.SpecializationRepository.ExistsAsync(s => s.Id == request.SpecializationId, cancellationToken);
            if (!specializationExists)
                throw new NotFoundException(LocalizationKeys.SpecializationMessages.NotFound.Value);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var user = ApplicationUser.Create(
                    request.FullName,
                    request.Email,
                    request.PhoneNumber,
                    request.BirthDate,
                    request.Gender);

                if (!string.IsNullOrEmpty(request.DoctorImage))
                    user.UpdateProfilePicture(request.DoctorImage);

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }

                await _userManager.AddToRoleAsync(user, UserType.Doctor.ToString());

                user.AssignToClinic(request.ClinicId);

                var existingDoctor = await _unitOfWork.DoctorRepository
                    .GetAllAsync(d => d.UserId == user.Id && d.ClinicId == request.ClinicId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingDoctor != null)
                    throw new BadRequestException(LocalizationKeys.DoctorMessages.AlreadyExistsInClinic.Value);

                var doctor = new Doctor(
                    user.Id,
                    request.ClinicId,
                    request.SpecializationId,
                    request.Bio ?? string.Empty,
                    request.YearsOfExperience);

                await _unitOfWork.DoctorRepository.AddAsync(doctor);

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
                await _unitOfWork.CommitAsync();

                return _mapper.Map<DoctorDto>(doctor);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
