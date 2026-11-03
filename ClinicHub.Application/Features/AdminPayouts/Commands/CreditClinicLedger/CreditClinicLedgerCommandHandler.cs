using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.AdminPayouts.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CreditClinicLedger;

public class CreditClinicLedgerCommandHandler
    : IRequestHandler<CreditClinicLedgerCommand, ClinicLedgerCreditDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreditClinicLedgerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClinicLedgerCreditDto> Handle(
        CreditClinicLedgerCommand request,
        CancellationToken cancellationToken)
    {
        var clinic = await _unitOfWork.ClinicRepository.FindByKeyAsync(request.ClinicId, cancellationToken)
            ?? throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

        var repository = _unitOfWork.GetRepository<ClinicLedgerEntry, Guid>();

        var entry = ClinicLedgerEntry.CreateAdjustment(clinic.Id, request.Amount, "EGP", request.Notes);
        await repository.AddAsync(entry);
        await _unitOfWork.SaveChangesAsync();

        var now = DateTime.Now;
        var available = await repository
            .GetAllAsync(e => e.ClinicId == clinic.Id
                && e.Status == LedgerEntryStatus.Available
                && e.AvailableAt <= now)
            .SumAsync(e => e.Amount, cancellationToken);

        return new ClinicLedgerCreditDto
        {
            EntryId = entry.Id,
            ClinicId = clinic.Id,
            ClinicName = clinic.Name,
            Amount = entry.Amount,
            Currency = entry.Currency,
            AvailableBalance = available
        };
    }
}
