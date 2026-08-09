using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;

namespace ClinicHub.Application.Features.Ratings
{
    internal static class RatingValidationHelper
    {
        public static async Task<Doctor> GetValidatedDoctorAsync(
            IUnitOfWork unitOfWork, Guid userId, Guid doctorId, CancellationToken cancellationToken)
        {
            var doctor = await unitOfWork.DoctorRepository
                .GetFirstAsync(d => d.Id == doctorId && !d.IsDeleted, cancellationToken);
            if (doctor == null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            if (doctor.UserId == userId)
                throw new BadRequestException(LocalizationKeys.RatingMessages.CannotRateSelf.Value);

            await EnsureCompletedVisitAsync(unitOfWork, userId, doctorId, null, cancellationToken);

            return doctor;
        }

        public static async Task ValidateClinicAsync(
            IUnitOfWork unitOfWork, Guid userId, Guid clinicId, CancellationToken cancellationToken)
        {
            var clinicExists = await unitOfWork.ClinicRepository
                .ExistsAsync(c => c.Id == clinicId && c.IsActive && !c.IsDeleted, cancellationToken);
            if (!clinicExists)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            await EnsureCompletedVisitAsync(unitOfWork, userId, null, clinicId, cancellationToken);
        }

        private static async Task EnsureCompletedVisitAsync(
            IUnitOfWork unitOfWork, Guid userId, Guid? doctorId, Guid? clinicId, CancellationToken cancellationToken)
        {
            var hasVisit = await unitOfWork.AppointmentRepository.ExistsAsync(a =>
                a.BookedByUserId == userId
                && a.Status == AppointmentStatus.Completed
                && !a.IsDeleted
                && (doctorId == null || a.DoctorId == doctorId.Value)
                && (clinicId == null || a.ClinicId == clinicId.Value), cancellationToken);

            if (!hasVisit)
                throw new BadRequestException(LocalizationKeys.RatingMessages.NoCompletedVisit.Value);
        }
    }
}
