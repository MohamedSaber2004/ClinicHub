using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutPreview;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CreatePayoutBatch;

public class CreatePayoutBatchCommandHandler
    : IRequestHandler<CreatePayoutBatchCommand, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IClinicLedgerService _ledger;
    private readonly IRequestHandler<GetPayoutPreviewQuery, PayoutPreviewDto> _preview;
    private readonly PayoutSettings _payoutSettings;

    public CreatePayoutBatchCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IClinicLedgerService ledger,
        IRequestHandler<GetPayoutPreviewQuery, PayoutPreviewDto> preview,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _ledger = ledger;
        _preview = preview;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        CreatePayoutBatchCommand request,
        CancellationToken cancellationToken)
    {
        // The batch's author is recorded so approval can check it is a different person.
        var actorId = _currentUserService.UserId;

        var preview = await _preview.Handle(
            new GetPayoutPreviewQuery { MinimumAmount = request.MinimumAmount },
            cancellationToken);

        var eligible = preview.Items
            .Where(i => i.Eligible && (request.ClinicIds is null || request.ClinicIds.Contains(i.ClinicId)))
            .ToList();

        if (eligible.Count == 0)
            throw new BadRequestException(LocalizationKeys.PayoutMessages.NoEligibleClinics.Value);

        var accounts = await _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>()
            .GetAllAsync(a => eligible.Select(e => e.ClinicId).Contains(a.ClinicId)
                && a.Status == PayoutAccountStatus.Verified)
            .ToListAsync(cancellationToken);

        var batch = PayoutBatch.Create(
            await NextCodeAsync(cancellationToken),
            await PeriodFromAsync(cancellationToken),
            DateTime.Now,
            actorId);

        foreach (var clinic in eligible)
        {
            var account = accounts.FirstOrDefault(a => a.ClinicId == clinic.ClinicId);
            if (account is null)
                continue;

            // The item id exists before the row does: ledger rows are reserved by it, which is
            // what makes paying the same earning twice a database impossibility.
            var itemId = Guid.NewGuid();
            var reserved = await _ledger.ReserveForPayoutItemAsync(clinic.ClinicId, itemId, cancellationToken);

            if (reserved < _payoutSettings.MinimumTransactionAmount)
            {
                await _ledger.ReleasePayoutItemAsync(itemId, cancellationToken);
                continue;
            }

            batch.AddItem(clinic.ClinicId, account.Id, reserved, clinic.Currency, itemId);
        }

        if (batch.ItemsCount == 0)
            throw new BadRequestException(LocalizationKeys.PayoutMessages.NoEligibleClinics.Value);

        await _unitOfWork.GetRepository<PayoutBatch, Guid>().AddAsync(batch);
        await _unitOfWork.SaveChangesAsync();

        return PayoutBatchDto.From(batch, _payoutSettings.MaxRetriesPerItem);
    }

    private async Task<string> NextCodeAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.Now.Date;
        var sequence = await _unitOfWork.GetRepository<PayoutBatch, Guid>()
            .GetAllAsync(b => b.CreatedAt >= today)
            .CountAsync(cancellationToken) + 1;

        return $"PO-{today:yyyy-MM-dd}-{sequence:00}";
    }

    /// <summary>Starts where the previous run stopped, so statements never overlap.</summary>
    private async Task<DateTime> PeriodFromAsync(CancellationToken cancellationToken)
    {
        var lastCompleted = await _unitOfWork.GetRepository<PayoutBatch, Guid>()
            .GetAllAsync(b => b.CompletedAt != null)
            .OrderByDescending(b => b.CompletedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return lastCompleted?.PeriodTo ?? todayMinus(_payoutSettings.RefundHoldDays);
    }

    private static DateTime todayMinus(int days) => DateTime.Now.Date.AddDays(-days);
}
