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

namespace ClinicHub.Application.Features.AdminPayouts.Commands.ApprovePayoutBatch;

public class ApprovePayoutBatchCommandHandler
    : IRequestHandler<ApprovePayoutBatchCommand, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PayoutSettings _payoutSettings;

    public ApprovePayoutBatchCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        ApprovePayoutBatchCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = _currentUserService.UserId;

        var repository = _unitOfWork.GetRepository<PayoutBatch, Guid>();

        var batch = await repository.GetAllAsync(b => b.Id == request.BatchId)
            .Include(b => b.Items)
                .ThenInclude(i => i.Clinic)
            .Include(b => b.Items)
                .ThenInclude(i => i.PayoutAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.BatchNotFound.Value);

        // Approving is the last human gate before money moves, so a self-approval is refused
        // instead of silently allowed when the two-man rule is on.
        if (!batch.Approve(actorId, _payoutSettings.RequireTwoManApproval))
            throw new BadRequestException(LocalizationKeys.PayoutMessages.BatchNotApprovable.Value);

        repository.Update(batch);
        await _unitOfWork.SaveChangesAsync();

        return PayoutBatchDto.From(batch, _payoutSettings.MaxRetriesPerItem);
    }
}
