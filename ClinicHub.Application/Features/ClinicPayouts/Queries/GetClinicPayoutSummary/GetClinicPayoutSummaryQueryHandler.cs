using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.ClinicPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutSummary;

public class GetClinicPayoutSummaryQueryHandler
    : IRequestHandler<GetClinicPayoutSummaryQuery, ClinicPayoutSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PayoutSettings _payoutSettings;

    public GetClinicPayoutSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<ClinicPayoutSummaryDto> Handle(
        GetClinicPayoutSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.CurrentClinicId
            ?? throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var entries = await _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>()
            .GetAllAsync(e => e.ClinicId == clinicId)
            .Select(e => new { e.Type, e.Status, e.Amount, e.AvailableAt })
            .ToListAsync(cancellationToken);

        var now = DateTime.Now;

        var accountStatus = await _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>()
            .GetAllAsync(a => a.ClinicId == clinicId)
            .Select(a => (PayoutAccountStatus?)a.Status)
            .FirstOrDefaultAsync(cancellationToken);

        var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new ClinicPayoutSummaryDto
        {
            AvailableBalance = entries
                .Where(e => e.Status == LedgerEntryStatus.Available && e.AvailableAt <= now)
                .Sum(e => e.Amount),
            PendingBalance = entries
                .Where(e => e.Status == LedgerEntryStatus.Available && e.AvailableAt > now)
                .Sum(e => e.Amount),
            ReservedBalance = entries
                .Where(e => e.Status == LedgerEntryStatus.Reserved)
                .Sum(e => e.Amount),
            PaidTotal = Math.Abs(entries
                .Where(e => e.Type == LedgerEntryType.Payout && e.Status == LedgerEntryStatus.Settled)
                .Sum(e => e.Amount)),
            MinimumPayoutAmount = setting?.MinimumPayoutAmount ?? _payoutSettings.MinimumPayoutAmount,
            AccountStatus = accountStatus,
            IsPayoutEnabled = accountStatus == PayoutAccountStatus.Verified
        };
    }
}
