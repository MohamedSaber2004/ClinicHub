using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutPreview;

public class GetPayoutPreviewQueryHandler
    : IRequestHandler<GetPayoutPreviewQuery, PayoutPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutSettings _payoutSettings;

    public GetPayoutPreviewQueryHandler(IUnitOfWork unitOfWork, IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutPreviewDto> Handle(
        GetPayoutPreviewQuery request,
        CancellationToken cancellationToken)
    {
        var minimum = request.MinimumAmount ?? _payoutSettings.MinimumPayoutAmount;
        if (minimum < _payoutSettings.MinimumTransactionAmount)
            minimum = _payoutSettings.MinimumTransactionAmount;

        var now = DateTime.Now;

        var balances = await _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>()
            .GetAllAsync(e => e.Status == LedgerEntryStatus.Available && e.AvailableAt <= now)
            .GroupBy(e => e.ClinicId)
            .Select(g => new
            {
                ClinicId = g.Key,
                Balance = g.Sum(x => x.Amount),
                Currency = g.Select(x => x.Currency).First()
            })
            .ToListAsync(cancellationToken);

        balances = balances.Where(b => b.Balance > 0).ToList();

        var preview = new PayoutPreviewDto { MinimumAmount = minimum };
        if (balances.Count == 0)
            return preview;

        var clinicIds = balances.Select(b => b.ClinicId).ToList();

        var clinics = await _unitOfWork.ClinicRepository
            .GetAllAsync(c => clinicIds.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        var accounts = await _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>()
            .GetAllAsync(a => clinicIds.Contains(a.ClinicId))
            .Select(a => new { a.ClinicId, a.Status, a.AccountLast4 })
            .ToListAsync(cancellationToken);

        var clinicNames = clinics.ToDictionary(c => c.Id, c => c.Name);
        var accountByClinic = accounts.ToDictionary(a => a.ClinicId);

        foreach (var balance in balances.OrderByDescending(b => b.Balance))
        {
            accountByClinic.TryGetValue(balance.ClinicId, out var account);

            string? ineligibilityReason = null;
            if (account is null)
                ineligibilityReason = "No payout account on file.";
            else if (account.Status != PayoutAccountStatus.Verified)
                ineligibilityReason = $"Payout account is {account.Status}.";
            else if (balance.Balance < minimum)
                ineligibilityReason = $"Balance is below the minimum of {minimum:0.00}.";

            preview.Items.Add(new PayoutPreviewItemDto
            {
                ClinicId = balance.ClinicId,
                ClinicName = clinicNames.GetValueOrDefault(balance.ClinicId, string.Empty),
                AvailableBalance = balance.Balance,
                Currency = balance.Currency,
                Eligible = ineligibilityReason is null,
                IneligibilityReason = ineligibilityReason,
                AccountStatus = account?.Status,
                AccountLast4 = account?.AccountLast4
            });
        }

        preview.TotalEligible = preview.Items.Where(i => i.Eligible).Sum(i => i.AvailableBalance);
        return preview;
    }
}
