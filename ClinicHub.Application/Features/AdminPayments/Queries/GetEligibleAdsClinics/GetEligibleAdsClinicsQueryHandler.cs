using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetEligibleAdsClinics;

public class GetEligibleAdsClinicsQueryHandler : IRequestHandler<GetEligibleAdsClinicsQuery, List<EligibleClinicDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEligibleAdsClinicsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<EligibleClinicDto>> Handle(GetEligibleAdsClinicsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return await _unitOfWork.GetRepository<Subscription, Guid>()
            .GetAllAsync(s => s.Status == SubscriptionStatus.Active && s.EndDate > now)
            .Include(s => s.Clinic)
            .Include(s => s.Plan)
                .ThenInclude(p => p!.Permissions)
            .Where(s => s.Clinic != null
                && s.Clinic.Status == ClinicStatus.Active
                && s.Plan != null
                && s.Plan.Permissions.Any(pp => pp.Permission == SubscriptionPermission.AdvancedReports))
            .Select(s => new EligibleClinicDto
            {
                Id = s.ClinicId,
                Name = s.Clinic!.Name,
                Email = s.Clinic.Email,
                Phone = s.Clinic.Phone
            })
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
