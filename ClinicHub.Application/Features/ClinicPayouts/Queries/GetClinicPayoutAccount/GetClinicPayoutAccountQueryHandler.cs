using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.ClinicPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.ClinicPayouts.Queries.GetClinicPayoutAccount;

public class GetClinicPayoutAccountQueryHandler
    : IRequestHandler<GetClinicPayoutAccountQuery, ClinicPayoutAccountDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetClinicPayoutAccountQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ClinicPayoutAccountDto?> Handle(
        GetClinicPayoutAccountQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.CurrentClinicId
            ?? throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var account = await _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>()
            .GetAllAsync(a => a.ClinicId == clinicId)
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
            return null;

        return new ClinicPayoutAccountDto
        {
            Id = account.Id,
            HolderName = account.HolderName,
            BankCode = account.BankCode,
            MaskedAccountNumber = $"**** {account.AccountLast4}",
            MaskedNationalId = $"**** {account.NationalIdLast4}",
            BankTransactionType = account.BankTransactionType,
            Status = account.Status,
            VerifiedAt = account.VerifiedAt,
            RejectionReason = account.RejectionReason
        };
    }
}
