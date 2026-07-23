using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Subscriptions.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Subscriptions.Queries.GetMyClinicSubscription
{
    public class GetMyClinicSubscriptionQueryHandler : IRequestHandler<GetMyClinicSubscriptionQuery, SubscriptionDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetMyClinicSubscriptionQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<SubscriptionDto?> Handle(GetMyClinicSubscriptionQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null) return null;

            var subscription = await _unitOfWork.GetRepository<Subscription, Guid>()
                .GetAllWithIncluding(s => s.ClinicId == clinicId && s.Status == SubscriptionStatus.Active, s => s.Clinic, s => s.Plan)
                .FirstOrDefaultAsync(cancellationToken);

            return subscription == null ? null : _mapper.Map<SubscriptionDto>(subscription);
        }
    }
}
