using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.Commands.ReconcilePayoutItems;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ReconcilePayoutBatch;

public class ReconcilePayoutBatchCommandHandler
    : IRequestHandler<ReconcilePayoutBatchCommand, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly PayoutSettings _payoutSettings;

    public ReconcilePayoutBatchCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        ReconcilePayoutBatchCommand request,
        CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.GetRepository<PayoutBatch, Guid>();

        if (!await repository.ExistsAsync(b => b.Id == request.BatchId, cancellationToken))
            throw new NotFoundException(LocalizationKeys.PayoutMessages.BatchNotFound.Value);

        await _mediator.Send(
            new ReconcilePayoutItemsCommand { BatchId = request.BatchId }, cancellationToken);

        var batch = await repository.GetAllAsync(b => b.Id == request.BatchId)
            .Include(b => b.Items)
                .ThenInclude(i => i.Clinic)
            .Include(b => b.Items)
                .ThenInclude(i => i.PayoutAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.BatchNotFound.Value);

        return PayoutBatchDto.From(batch, _payoutSettings.MaxRetriesPerItem);
    }
}
