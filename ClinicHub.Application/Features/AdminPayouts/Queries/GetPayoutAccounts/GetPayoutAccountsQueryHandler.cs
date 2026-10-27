using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutAccounts;

public class GetPayoutAccountsQueryHandler
    : IRequestHandler<GetPayoutAccountsQuery, List<PayoutAccountAdminDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPayoutAccountsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PayoutAccountAdminDto>> Handle(
        GetPayoutAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>()
            .GetAllAsync(null)
            .Include(a => a.Clinic)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(a =>
                a.HolderName.Contains(term) ||
                (a.Clinic != null && a.Clinic.Name.Contains(term)));
        }

        var accounts = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return accounts
            .Select(a => PayoutAccountAdminDto.From(a, a.Clinic?.Name ?? string.Empty))
            .ToList();
    }
}
