using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetAdminDashboardStats
{
    public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;

        public GetAdminDashboardStatsQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserTypes is null || (_currentUser.UserTypes.Value & (int)UserType.SuperAdmin) == 0)
                throw new UnAuthorizedException();
            var verificationRequestsCount = await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.Status == VerificationStatus.Pending).CountAsync(cancellationToken);

            var activeClinicsCount = await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.IsActive && !c.IsDeleted).CountAsync(cancellationToken);

            var totalUsersCount = await _userManager.Users
                .CountAsync(u => !u.IsDeleted, cancellationToken);

            var supportTicketsCount = await _unitOfWork.GetRepository<SupportTicket, Guid>()
                .GetAllAsync(t => t.Status != SupportTicketStatus.Closed).CountAsync(cancellationToken);

            var urgentSupportTicketsCount = await _unitOfWork.GetRepository<SupportTicket, Guid>()
                .GetAllAsync(t => t.Priority == SupportTicketPriority.Urgent && t.Status != SupportTicketStatus.Closed)
                .CountAsync(cancellationToken);

            var specializationsCount = await _unitOfWork.SpecializationRepository
                .GetAllAsync(s => !s.IsDeleted).CountAsync(cancellationToken);

            var activeAdsCount = await _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllAsync(a => a.Status == AdvertisementStatus.Active).CountAsync(cancellationToken);

            var revokedSubscriptionsCount = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.Status == SubscriptionStatus.Revoked).CountAsync(cancellationToken);

            return new AdminDashboardStatsDto
            {
                VerificationRequestsCount = verificationRequestsCount,
                ActiveClinicsCount = activeClinicsCount,
                TotalUsersCount = totalUsersCount,
                SupportTicketsCount = supportTicketsCount,
                UrgentSupportTicketsCount = urgentSupportTicketsCount,
                SpecializationsCount = specializationsCount,
                ActiveAdsCount = activeAdsCount,
                RevokedSubscriptionsCount = revokedSubscriptionsCount
            };
        }
    }
}
