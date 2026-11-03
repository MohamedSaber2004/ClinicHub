using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ExecutePayoutBatch;

public class ExecutePayoutBatchCommandHandler
    : IRequestHandler<ExecutePayoutBatchCommand, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBackgroundJobScheduler _jobScheduler;
    private readonly PayoutSettings _payoutSettings;

    public ExecutePayoutBatchCommandHandler(
        IUnitOfWork unitOfWork,
        IBackgroundJobScheduler jobScheduler,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _jobScheduler = jobScheduler;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        ExecutePayoutBatchCommand request,
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

        if (batch.Status != PayoutBatchStatus.Approved)
            throw new BadRequestException(LocalizationKeys.PayoutMessages.BatchNotExecutable.Value);

        if (batch.ItemsCount == 0)
            throw new BadRequestException(LocalizationKeys.PayoutMessages.NoEligibleClinics.Value);

        await _jobScheduler.SchedulePayoutExecutionAsync(batch.Id);

        return PayoutBatchDto.From(batch, _payoutSettings.MaxRetriesPerItem);
    }
}
