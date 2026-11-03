using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CancelPayoutBatch;

public class CancelPayoutBatchCommandHandler
    : IRequestHandler<CancelPayoutBatchCommand, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClinicLedgerService _ledger;
    private readonly PayoutSettings _payoutSettings;

    public CancelPayoutBatchCommandHandler(
        IUnitOfWork unitOfWork,
        IClinicLedgerService ledger,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _ledger = ledger;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        CancelPayoutBatchCommand request,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutBatch, Guid>();

        var batch = await repository.GetAllAsync(b => b.Id == request.BatchId)
            .Include(b => b.Items)
                .ThenInclude(i => i.Clinic)
            .Include(b => b.Items)
                .ThenInclude(i => i.PayoutAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.BatchNotFound.Value);

        // A run that already started may hold live transfers; only a finished-or-failed batch
        // can be cancelled, and never one in Processing.
        if (!batch.Cancel())
            throw new BadRequestException(LocalizationKeys.PayoutMessages.BatchNotCancellable.Value);

        foreach (var item in batch.Items.Where(i => !i.IsResolved))
        {
            item.Cancel();
            await _ledger.ReleasePayoutItemAsync(item.Id, cancellationToken);
        }

        repository.Update(batch);
        await _unitOfWork.SaveChangesAsync();

        return PayoutBatchDto.From(batch, _payoutSettings.MaxRetriesPerItem);
    }
}
