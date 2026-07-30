using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.ClinicStaff.DTOs;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Features.Ratings.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicDetails
{
    public class GetClinicDetailsQueryHandler : IRequestHandler<GetClinicDetailsQuery, ClinicDetailsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetClinicDetailsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ClinicDetailsDto> Handle(GetClinicDetailsQuery request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.Id == request.Id)
                .IgnoreQueryFilters()
                .Include(c => c.Specialization)
                .Include(c => c.ClinicAdmin)
                .FirstOrDefaultAsync(cancellationToken);

            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            var dto = _mapper.Map<ClinicDetailsDto>(clinic);

            if (clinic.ClinicAdmin != null)
            {
                dto.OwnerName = clinic.ClinicAdmin.FullName;
                dto.OwnerEmail = clinic.ClinicAdmin.Email;
                dto.OwnerPhone = clinic.ClinicAdmin.PhoneNumber;
            }

            var doctors = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.ClinicId == request.Id && !d.IsDeleted)
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .ToListAsync(cancellationToken);

            dto.Doctors = doctors.Select(d => new ClinicDoctorDto
            {
                Id = d.Id,
                Name = d.User.FullName,
                Image = d.User.ProfilePictureUrl,
                Phone = d.User.PhoneNumber,
                Email = d.User.Email,
                SpecializationArName = d.Specialization.ArName,
                SpecializationEnName = d.Specialization.Name,
                Bio = d.Bio,
                YearsOfExperience = d.YearsOfExperience
            }).ToList();

            var staffUsers = await _userManager.GetUsersInRoleAsync(UserType.Staff.ToString());
            var clinicStaff = staffUsers
                .Where(u => u.ClinicId == request.Id && !u.IsDeleted)
                .Select(u => new StaffDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                }).ToList();

            dto.Staff = clinicStaff;

            var ratings = await _unitOfWork.GetRepository<Rating, Guid>()
                .GetAllAsync(r => r.ClinicId == request.Id && !r.IsDeleted)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);

            dto.AverageRating = ratings.Any() ? Math.Round(ratings.Average(r => r.Value), 1) : null;
            dto.TotalRatings = ratings.Count;
            dto.RecentRatings = ratings.Take(10).Select(r => new RatingDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User.FullName,
                DoctorId = r.DoctorId,
                ClinicId = r.ClinicId,
                Value = r.Value,
                Review = r.Review,
                CreatedAt = r.CreatedAt
            }).ToList();

            return dto;
        }
    }
}
