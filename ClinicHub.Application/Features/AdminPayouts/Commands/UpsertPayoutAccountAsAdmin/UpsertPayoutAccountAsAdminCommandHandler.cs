using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Common;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.UpsertPayoutAccountAsAdmin;

public class UpsertPayoutAccountAsAdminCommandHandler
    : IRequestHandler<UpsertPayoutAccountAsAdminCommand, PayoutAccountAdminDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPayoutAccountProtector _protector;

    public UpsertPayoutAccountAsAdminCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IPayoutAccountProtector protector)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _protector = protector;
    }

    public async Task<PayoutAccountAdminDto> Handle(
        UpsertPayoutAccountAsAdminCommand request,
        CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId, cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var destination = SanitizeDestination(request.AccountNumber);
        var nationalId = new string(request.NationalId.Where(char.IsDigit).ToArray());

        var repository = _unitOfWork.GetRepository<ClinicPayoutAccount, Guid>();

        var account = await repository
            .GetAllAsync(a => a.ClinicId == clinic.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            account = new ClinicPayoutAccount(
                clinic.Id,
                request.HolderName,
                _protector.Protect(nationalId),
                nationalId[^4..],
                request.BankCode,
                _protector.Protect(destination),
                destination[^4..],
                PayoutBankTransactionTypes.Normalize(request.BankTransactionType));

            await repository.AddAsync(account);
        }
        else
        {
            account.UpdateDetails(
                request.HolderName,
                _protector.Protect(nationalId),
                nationalId[^4..],
                request.BankCode,
                _protector.Protect(destination),
                destination[^4..],
                PayoutBankTransactionTypes.Normalize(request.BankTransactionType));

            repository.Update(account);
        }

        account.Verify(_currentUserService.UserId);

        await _unitOfWork.SaveChangesAsync();

        return PayoutAccountAdminDto.From(account, clinic.Name);
    }

    /// <summary>Account numbers, IBANs and card numbers are stored exactly as the bank issued them.</summary>
    private static string SanitizeDestination(string accountNumber) =>
        new string(accountNumber.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
