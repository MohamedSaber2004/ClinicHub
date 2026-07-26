using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorDetailsForMobile
{
    public class GetDoctorDetailsForMobileQueryHandler : IRequestHandler<GetDoctorDetailsForMobileQuery, DoctorDetailsForMobileDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetDoctorDetailsForMobileQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorDetailsForMobileDto> Handle(GetDoctorDetailsForMobileQuery request, CancellationToken cancellationToken)
        {
            var doctor = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.Id == request.DoctorId)
                .Include(d => d.User)
                .Include(d => d.Clinic)
                .Include(d => d.Specialization)
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(cancellationToken);

            if (doctor is null)
                throw new NotFoundException(LocalizationKeys.DoctorMessages.NotFound.Value);

            // Sequential queries — EF Core DbContext is not thread-safe,
            // Task.WhenAll on the same context causes a concurrency exception.
            var allRatings = await _unitOfWork.RatingRepository.GetDoctorRatingsAsync(request.DoctorId);
            var averageRating = await _unitOfWork.RatingRepository.GetDoctorAverageRatingAsync(request.DoctorId);

            var recentRatings = allRatings
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            return new DoctorDetailsForMobileDto
            {
                Id = doctor.Id,
                FullName = doctor.User.FullName,
                ProfilePictureUrl = doctor.User.ProfilePictureUrl,
                Bio = doctor.Bio,
                YearsOfExperience = doctor.YearsOfExperience,
                IsFreelance = doctor.IsFreelance,
                ClinicId = doctor.ClinicId,
                ClinicName = doctor.Clinic?.Name,
                ClinicNameAr = doctor.Clinic?.NameAr,
                SpecializationId = doctor.SpecializationId,
                SpecializationName = doctor.Specialization?.ArName,
                AverageRating = averageRating,
                TotalRatings = allRatings.Count,
                RecentRatings = recentRatings.Select(r => new DoctorRatingSummaryDto
                {
                    Id = r.Id,
                    ReviewerName = r.User.FullName,
                    ReviewerProfilePictureUrl = r.User.ProfilePictureUrl,
                    Value = r.Value,
                    Review = r.Review,
                    CreatedAt = r.CreatedAt
                }).ToList(),
                Availabilities = doctor.Availabilities
                    .OrderBy(a => a.DayOfWeek)
                    .Select(a => new DoctorAvailabilityDto
                    {
                        Id = a.Id,
                        DayOfWeek = a.DayOfWeek,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        SlotDurationMinutes = a.SlotDurationMinutes
                    }).ToList()
            };
        }
    }
}
