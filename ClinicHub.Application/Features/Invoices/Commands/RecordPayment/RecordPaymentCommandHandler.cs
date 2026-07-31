using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.RecordPayment;

public class RecordPaymentCommandHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IStringLocalizer<Messages> localizer
) : IRequestHandler<RecordPaymentCommand, Guid>
{
    public async Task<Guid> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithItemsAsync(request.InvoiceId)
            ?? throw new NotFoundException(localizer[LocalizationKeys.InvoiceMessages.NotFound]);

        var clinicId = currentUserService.CurrentClinicId;
        if (clinicId != null && invoice.ClinicId != clinicId.Value)
            throw new ForbiddenException(localizer[LocalizationKeys.ExceptionMessages.Forbidden]);

        if (invoice.Status != InvoiceStatus.Issued)
            throw new BadRequestException(localizer[LocalizationKeys.InvoiceMessages.InvalidStatus]);

        var payment = new ClinicHub.Domain.Entities.Payment(null, currentUserService.UserId, invoice.ClinicId, request.Amount);
        payment.MarkAsPaid(request.TransactionRef ?? Guid.NewGuid().ToString(), request.Method.ToString());

        await unitOfWork.PaymentRepository.AddAsync(payment);

        invoice.MarkAsPaid();
        unitOfWork.InvoiceRepository.Update(invoice);

        await unitOfWork.SaveChangesAsync();

        return payment.Id;
    }
}
