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

namespace ClinicHub.Application.Features.AdminPayouts.Commands.RetryPayoutItem;

public class RetryPayoutItemCommandHandler
    : IRequestHandler<RetryPayoutItemCommand, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobScheduler _jobScheduler;
    private readonly PayoutSettings _payoutSettings;

    public RetryPayoutItemCommandHandler(
        IUnitOfWork unitOfWork,
        IBackgroundJobScheduler jobScheduler,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        RetryPayoutItemCommand request,
        CancellationToken cancellationToken)
    {
        var itemRepository = _unitOfWork.GetRepository<PayoutItem, Guid>();

        var item = await itemRepository.GetAllAsync(i => i.Id == request.ItemId)
            .Include(i => i.Batch)
                .ThenInclude(b => b.Items)
            .Include(i => i.Clinic)
            .Include(i => i.PayoutAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.ItemNotFound.Value);

        if (!item.QueueForRetry(_payoutSettings.MaxRetriesPerItem))
            throw new BadRequestException(LocalizationKeys.PayoutMessages.ItemNotRetryable.Value);

        itemRepository.Update(item);
        _unitOfWork.GetRepository<PayoutBatch, Guid>().Update(item.Batch);

        // A closed run becomes live again the moment one of its items is re-queued.
        item.Batch.ReopenForRetry();

        await _unitOfWork.SaveChangesAsync();
        await _jobScheduler.SchedulePayoutExecutionAsync(item.BatchId);

        return PayoutBatchDto.From(item.Batch, _payoutSettings.MaxRetriesPerItem);
    }
}
