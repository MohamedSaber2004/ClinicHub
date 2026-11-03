using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBatchDetails;

public class GetPayoutBatchDetailsQueryHandler
    : IRequestHandler<GetPayoutBatchDetailsQuery, PayoutBatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutSettings _payoutSettings;

    public GetPayoutBatchDetailsQueryHandler(
        IUnitOfWork unitOfWork,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PayoutBatchDto> Handle(
        GetPayoutBatchDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.GetRepository<PayoutBatch, Guid>()
            .GetAllAsync(b => b.Id == request.BatchId)
            .Include(b => b.Items)
                .ThenInclude(i => i.Clinic)
            .Include(b => b.Items)
                .ThenInclude(i => i.PayoutAccount)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.PayoutMessages.BatchNotFound.Value);

        return PayoutBatchDto.From(batch, _payoutSettings.MaxRetriesPerItem);
    }
}
