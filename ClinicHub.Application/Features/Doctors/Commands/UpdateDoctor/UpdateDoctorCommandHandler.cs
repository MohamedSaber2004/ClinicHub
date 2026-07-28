using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor
{
    public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, DoctorDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateDoctorCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<DoctorDto> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var doctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(null)
                .IgnoreQueryFilters()
                .Include(d => d.User)
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

            if (doctor == null || doctor.ClinicId != clinicId)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var user = doctor.User;

                if (request.FullName != null)
                    user.UpdateFullName(request.FullName);

                if (request.PhoneNumber != null)
                    user.UpdatePhoneNumber(request.PhoneNumber);

                if (request.Email != null)
                {
                    var emailOwner = await _userManager.FindByEmailAsync(request.Email);
                    if (emailOwner != null && emailOwner.Id != user.Id && !emailOwner.IsDeleted)
                        throw new BadRequestException(LocalizationKeys.AuthMessages.EmailAlreadyExists.Value);

                    await _userManager.SetEmailAsync(user, request.Email);
                    await _userManager.SetUserNameAsync(user, request.Email);
                }

                if (request.BirthDate.HasValue)
                    user.UpdateBirthDate(request.BirthDate);

                if (request.Gender.HasValue)
                    user.UpdateGender(request.Gender.Value);

                if (request.DoctorImage != null)
                    user.UpdateProfilePicture(request.DoctorImage);

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

                if (request.Availabilities.Count > 0)
                {
                    foreach (var existing in doctor.Availabilities.ToList())
                    {
                        _unitOfWork.DoctorAvailabilityRepository.Delete(existing);
                    }

                    foreach (var av in request.Availabilities)
                    {
                        var availability = new DoctorAvailability(
                            doctor.Id,
                            clinicId.Value,
                            av.DayOfWeek,
                            av.StartTime,
                            av.EndTime,
                            av.SlotDurationMinutes);

                        await _unitOfWork.DoctorAvailabilityRepository.AddAsync(availability);
                    }
                }

                _unitOfWork.DoctorRepository.Update(doctor);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var doctorDto = _mapper.Map<DoctorDto>(doctor);
            doctorDto.Availabilities = (await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == doctor.Id && !a.IsDeleted)
                .ToListAsync(cancellationToken))
                .Select(_mapper.Map<DoctorAvailabilityDto>)
                .ToList();

            return doctorDto;
        }
    }
}
