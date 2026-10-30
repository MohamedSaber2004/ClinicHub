using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutPreview;

public class GetPayoutPreviewQueryHandler
    : IRequestHandler<GetPayoutPreviewQuery, PayoutPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutSettings _payoutSettings;
    private readonly IStringLocalizer<Messages> _localizer;

    public GetPayoutPreviewQueryHandler(
        IUnitOfWork unitOfWork,
        IOptions<PayoutSettings> payoutSettings,
        IStringLocalizer<Messages> localizer)
    {
        _unitOfWork = unitOfWork;
        _payoutSettings = payoutSettings.Value;
        _localizer = localizer;
    }

    public async Task<PayoutPreviewDto> Handle(
        GetPayoutPreviewQuery request,
        CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var configuredMinimum = setting?.MinimumPayoutAmount ?? _payoutSettings.MinimumPayoutAmount;

        var minimum = request.MinimumAmount ?? configuredMinimum;
        if (minimum < _payoutSettings.MinimumTransactionAmount)
            minimum = _payoutSettings.MinimumTransactionAmount;

        var now = DateTime.Now;
        var ledger = _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>();

        var grouped = await ledger
            .GetAllAsync(e => e.Status == LedgerEntryStatus.Available
                && e.AvailableAt <= now
                && (request.ClinicId == null || e.ClinicId == request.ClinicId))
            .GroupBy(e => e.ClinicId)
            .Select(g => new
            {
                ClinicId = g.Key,
                Balance = g.Sum(x => x.Amount),
                Currency = g.Select(x => x.Currency).First()
            })
            .ToListAsync(cancellationToken);

        var balances = grouped
            .Where(b => b.Balance > 0)
            .Select(b => (b.ClinicId, b.Balance, Currency: b.Currency ?? "EGP"))
            .ToList();

        // An admin asking about one clinic still needs an answer when that clinic has nothing
        // available, so it is reported with a zero balance and the reason instead of vanishing.
        if (request.ClinicId is Guid targetedClinicId
            && balances.All(b => b.ClinicId != targetedClinicId))
        {
            balances.Add((targetedClinicId, 0m, "EGP"));
        }

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

        decimal pending = 0m, reserved = 0m;
        if (request.ClinicId is Guid heldClinicId)
        {
            pending = await ledger
                .GetAllAsync(e => e.ClinicId == heldClinicId
                    && e.Status == LedgerEntryStatus.Available
                    && e.AvailableAt > now)
                .SumAsync(e => e.Amount, cancellationToken);
            reserved = await ledger
                .GetAllAsync(e => e.ClinicId == heldClinicId
                    && e.Status == LedgerEntryStatus.Reserved)
                .SumAsync(e => e.Amount, cancellationToken);
        }

        var clinicNames = clinics.ToDictionary(c => c.Id, c => c.Name);
        var accountByClinic = accounts.ToDictionary(a => a.ClinicId);

        foreach (var balance in balances.OrderByDescending(b => b.Balance))
        {
            accountByClinic.TryGetValue(balance.ClinicId, out var account);

            string? ineligibilityReason = null;
            if (account is null)
                ineligibilityReason = _localizer[LocalizationKeys.PayoutMessages.IneligibleNoAccount].Value;
            else if (account.Status != PayoutAccountStatus.Verified)
                ineligibilityReason = _localizer[LocalizationKeys.PayoutMessages.IneligibleAccountNotVerified].Value;
            else if (balance.Balance <= 0)
                ineligibilityReason = _localizer[LocalizationKeys.PayoutMessages.IneligibleNoBalance].Value;
            else if (balance.Balance < minimum)
                ineligibilityReason = _localizer[
                    LocalizationKeys.PayoutMessages.IneligibleBelowMinimum, minimum.ToString("0.00")].Value;

            preview.Items.Add(new PayoutPreviewItemDto
            {
                ClinicId = balance.ClinicId,
                ClinicName = clinicNames.GetValueOrDefault(balance.ClinicId, string.Empty),
                AvailableBalance = balance.Balance,
                PendingBalance = pending,
                ReservedBalance = reserved,
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
