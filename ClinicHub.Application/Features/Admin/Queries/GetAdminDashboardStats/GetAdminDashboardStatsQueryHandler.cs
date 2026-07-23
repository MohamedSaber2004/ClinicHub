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

        public GetAdminDashboardStatsQueryHandler(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var totalUsers = _userManager.Users.Count(u => !u.IsDeleted);

            var verificationsQuery = _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.Status == VerificationStatus.Pending);

            var clinicsQuery = _unitOfWork.ClinicRepository
                .GetAllAsync(c => !c.IsDeleted && c.Status == ClinicStatus.Active);

            var ticketsQuery = _unitOfWork.GetRepository<SupportTicket, Guid>()
                .GetAllAsync(t => true);

            var adsQuery = _unitOfWork.GetRepository<Advertisement, Guid>()
                .GetAllAsync(a => a.Status == AdvertisementStatus.Active);

            var subscriptionsQuery = _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllAsync(s => s.Status == SubscriptionStatus.Active);

            var specializationsQuery = _unitOfWork.SpecializationRepository
                .GetAllAsync(s => true);

            var totalTickets = await ticketsQuery.CountAsync(cancellationToken);
            var urgentTickets = await ticketsQuery
                .CountAsync(t => t.Priority == SupportTicketPriority.Urgent
                    && (t.Status == SupportTicketStatus.Open || t.Status == SupportTicketStatus.InProgress),
                    cancellationToken);

            return new AdminDashboardStatsDto
            {
                TotalUsersCount = totalUsers,
                VerificationRequestsCount = await verificationsQuery.CountAsync(cancellationToken),
                ActiveClinicsCount = await clinicsQuery.CountAsync(cancellationToken),
                SupportTicketsCount = totalTickets,
                UrgentSupportTicketsCount = urgentTickets,
                SpecializationsCount = await specializationsQuery.CountAsync(cancellationToken),
                ActiveAdsCount = await adsQuery.CountAsync(cancellationToken),
                RevokedSubscriptionsCount = await subscriptionsQuery.CountAsync(cancellationToken)
            };
        }
    }
}
