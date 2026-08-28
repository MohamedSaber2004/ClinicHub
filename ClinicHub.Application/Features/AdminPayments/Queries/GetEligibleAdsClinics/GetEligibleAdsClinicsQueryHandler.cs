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
        // ADS INDEPENDENT: return all Active clinics, not only those with Active subscription
        return await _unitOfWork.ClinicRepository
            .GetAllAsync(c => c.Status == ClinicStatus.Active && !c.IsDeleted)
            .Select(c => new EligibleClinicDto
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone
            })
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
