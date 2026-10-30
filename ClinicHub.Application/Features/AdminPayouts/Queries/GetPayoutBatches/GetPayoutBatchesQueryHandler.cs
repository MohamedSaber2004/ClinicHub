using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutBatches;

public class GetPayoutBatchesQueryHandler
    : IRequestHandler<GetPayoutBatchesQuery, PagginatedResult<PayoutBatchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutSettings _payoutSettings;

    public GetPayoutBatchesQueryHandler(
        IUnitOfWork unitOfWork,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PagginatedResult<PayoutBatchDto>> Handle(
        GetPayoutBatchesQuery request,
        CancellationToken cancellationToken)
    {
        var batches = await _unitOfWork.GetRepository<PayoutBatch, Guid>()
            .GetAllAsync(b => request.Status == null || b.Status == request.Status)
            .Include(b => b.Items)
                .ThenInclude(i => i.Clinic)
            .Include(b => b.Items)
                .ThenInclude(i => i.PayoutAccount)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await _unitOfWork.GetRepository<PayoutBatch, Guid>()
            .GetAllAsync(b => request.Status == null || b.Status == request.Status)
            .CountAsync(cancellationToken);

        var dtos = batches
            .Select(b => PayoutBatchDto.From(b, _payoutSettings.MaxRetriesPerItem))
            .ToList();

        return PagginatedResult<PayoutBatchDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
