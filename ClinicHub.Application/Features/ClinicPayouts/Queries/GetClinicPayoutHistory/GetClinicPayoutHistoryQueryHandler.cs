using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutHistory;

public class GetClinicPayoutHistoryQueryHandler
    : IRequestHandler<GetClinicPayoutHistoryQuery, PagginatedResult<PayoutItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly PayoutSettings _payoutSettings;

    public GetClinicPayoutHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOptions<PayoutSettings> payoutSettings)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _payoutSettings = payoutSettings.Value;
    }

    public async Task<PagginatedResult<PayoutItemDto>> Handle(
        GetClinicPayoutHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.CurrentClinicId
            ?? throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var items = await _unitOfWork.GetRepository<PayoutItem, Guid>()
            .GetAllAsync(i => i.ClinicId == clinicId)
            .Include(i => i.Batch)
            .Include(i => i.Clinic)
            .Include(i => i.PayoutAccount)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var totalCount = await _unitOfWork.GetRepository<PayoutItem, Guid>()
            .GetAllAsync(i => i.ClinicId == clinicId)
            .CountAsync(cancellationToken);

        var dtos = items
            .Select(i => PayoutItemDto.From(i, _payoutSettings.MaxRetriesPerItem))
            .ToList();

        return PagginatedResult<PayoutItemDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
