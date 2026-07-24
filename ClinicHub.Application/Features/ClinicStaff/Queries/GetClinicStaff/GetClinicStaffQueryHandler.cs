using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.ClinicStaff.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.ClinicStaff.Queries.GetClinicStaff
{
    public class GetClinicStaffQueryHandler : IRequestHandler<GetClinicStaffQuery, PagginatedResult<StaffDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GetClinicStaffQueryHandler(ICurrentUserService currentUserService, UserManager<ApplicationUser> userManager)
        {
            _currentUserService = currentUserService;
            _userManager = userManager;
        }

        public async Task<PagginatedResult<StaffDto>> Handle(GetClinicStaffQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new PagginatedResult<StaffDto>(Array.Empty<StaffDto>(), 0);

            var staffInRole = await _userManager.GetUsersInRoleAsync(nameof(UserType.Staff));
            var clinicStaff = staffInRole
                .Where(u => u.ClinicId == clinicId)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                clinicStaff = clinicStaff.Where(u =>
                    (u.FullName != null && u.FullName.ToLower().Contains(term)) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)));
            }

            if (request.IsActive.HasValue)
                clinicStaff = clinicStaff.Where(u => u.IsActive == request.IsActive.Value);

            clinicStaff = clinicStaff.OrderByDescending(u => u.CreatedAt).ToList();

            var totalCount = clinicStaff.Count();
            var items = clinicStaff
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new StaffDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToList();

            return new PagginatedResult<StaffDto>(items, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
