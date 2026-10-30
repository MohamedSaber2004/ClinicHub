using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.ClinicPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.ClinicPayouts.Commands.UpsertClinicPayoutAccount;

public class UpsertClinicPayoutAccountCommandHandler
    : IRequestHandler<UpsertClinicPayoutAccountCommand, ClinicPayoutAccountDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPayoutAccountProtector _protector;

    public UpsertClinicPayoutAccountCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPayoutAccountProtector protector)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _protector = protector;
    }

    public async Task<ClinicPayoutAccountDto> Handle(
        UpsertClinicPayoutAccountCommand request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.CurrentClinicId
            ?? throw new ForbiddenException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var destination = SanitizeDestination(request.AccountNumber);
        var nationalId = new string(request.NationalId.Where(char.IsDigit).ToArray());
        var accountLast4 = destination[^4..];
        var nationalIdLast4 = nationalId[^4..];

        var accountRepository = _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>();

        var account = await accountRepository
            .GetAllAsync(a => a.ClinicId == clinicId)
            .FirstOrDefaultAsync(cancellationToken);

        var bankTransactionType = PayoutBankTransactionTypes.Normalize(request.BankTransactionType);
        var protectedDestination = _protector.Protect(destination);
        var protectedNationalId = _protector.Protect(nationalId);

        if (account is null)
        {
            account = new ClinicPayoutAccount(
                clinicId,
                request.HolderName,
                protectedNationalId,
                nationalIdLast4,
                request.BankCode,
                protectedDestination,
                accountLast4,
                bankTransactionType);

            await accountRepository.AddAsync(account);
        }
        else
        {
            account.UpdateDetails(
                request.HolderName,
                protectedNationalId,
                nationalIdLast4,
                request.BankCode,
                protectedDestination,
                accountLast4,
                bankTransactionType);

            accountRepository.Update(account);
        }

        await _unitOfWork.SaveChangesAsync();

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

    /// <summary>Account numbers, IBANs and card numbers are stored exactly as the bank issued them.</summary>
    private static string SanitizeDestination(string accountNumber) =>
        new string(accountNumber.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
