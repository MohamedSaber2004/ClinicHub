using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicHub.Application.Features.Doctors.Commands.DeleteDoctor
{
    public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteDoctorCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<bool> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                throw new BadRequestException(LocalizationKeys.ExceptionMessages.BadRequest.Value);

            var doctor = await _unitOfWork.DoctorRepository.GetByIdAsync(request.DoctorId);
            if (doctor == null || doctor.ClinicId != clinicId)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            var user = await _userManager.FindByIdAsync(doctor.UserId.ToString());
            if (user != null && !user.IsDeleted)
            {
                user.IsDeleted = true;
                user.IsActive = false;
                user.DeletedAt = DateTime.Now;
                await _userManager.UpdateAsync(user);
            }

            _unitOfWork.DoctorRepository.Delete(doctor);
            var result = await _unitOfWork.SaveChangesAsync();

            return result > 0;
        }
    }
}
