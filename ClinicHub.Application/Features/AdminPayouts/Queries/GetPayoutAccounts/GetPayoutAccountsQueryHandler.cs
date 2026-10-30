using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
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
        // Every clinic is listed, with or without a bank account, so the admin sees what is
        // still owed to it and what blocks the transfer on the same row.
        var clinics = await _unitOfWork.ClinicRepository
            .GetAllAsync(null)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        var accounts = await _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>()
            .GetAllAsync(null)
            .ToListAsync(cancellationToken);

        var now = DateTime.Now;

        var balances = await _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>()
            .GetAllAsync(e => e.Status != LedgerEntryStatus.Cancelled)
            .GroupBy(e => e.ClinicId)
            .Select(g => new
            {
                ClinicId = g.Key,
                Earned = g.Sum(x => x.Type == LedgerEntryType.Earning ? x.Amount : 0m),
                Available = g.Sum(x => x.Status == LedgerEntryStatus.Available && x.AvailableAt <= now ? x.Amount : 0m),
                Pending = g.Sum(x => x.Status == LedgerEntryStatus.Available && x.AvailableAt > now ? x.Amount : 0m),
                Reserved = g.Sum(x => x.Status == LedgerEntryStatus.Reserved ? x.Amount : 0m)
            })
            .ToListAsync(cancellationToken);

        var accountByClinic = accounts
            .GroupBy(a => a.ClinicId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.CreatedAt).First());
        var balanceByClinic = balances.ToDictionary(b => b.ClinicId);

        var term = request.Search?.Trim();

        return clinics
            .Select(c =>
            {
                accountByClinic.TryGetValue(c.Id, out var account);
                balanceByClinic.TryGetValue(c.Id, out var balance);

                return new PayoutAccountAdminDto
                {
                    Id = account?.Id ?? Guid.Empty,
                    ClinicId = c.Id,
                    ClinicName = c.Name,
                    HasAccount = account is not null,
                    HolderName = account?.HolderName ?? string.Empty,
                    BankCode = account?.BankCode ?? string.Empty,
                    MaskedAccountNumber = account is null ? string.Empty : $"**** {account.AccountLast4}",
                    BankTransactionType = account?.BankTransactionType ?? string.Empty,
                    Status = account?.Status,
                    SubmittedAt = account?.CreatedAt ?? default,
                    VerifiedAt = account?.VerifiedAt,
                    RejectionReason = account?.RejectionReason,
                    TotalEarned = balance?.Earned ?? 0m,
                    AvailableBalance = balance?.Available ?? 0m,
                    PendingBalance = balance?.Pending ?? 0m,
                    ReservedBalance = balance?.Reserved ?? 0m
                };
            })
            .Where(r => !request.Status.HasValue || r.Status == request.Status)
            .Where(r => string.IsNullOrWhiteSpace(term)
                || r.ClinicName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || r.HolderName.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(r => r.ClinicName)
            .ToList();
    }
}
