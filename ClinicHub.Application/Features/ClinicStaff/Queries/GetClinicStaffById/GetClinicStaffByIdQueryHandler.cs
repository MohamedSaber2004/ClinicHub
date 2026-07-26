using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.ClinicStaff.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.ClinicStaff.Queries.GetClinicStaffById
{
    public class GetClinicStaffByIdQueryHandler : IRequestHandler<GetClinicStaffByIdQuery, StaffDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public GetClinicStaffByIdQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<StaffDto> Handle(GetClinicStaffByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, cancellationToken);

            if (user == null)
                throw new NotFoundException(LocalizationKeys.StaffMessages.NotFound.Value);

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains(nameof(UserType.Staff)))
                throw new NotFoundException(LocalizationKeys.StaffMessages.NotFound.Value);

            return _mapper.Map<StaffDto>(user);
        }
    }
}
