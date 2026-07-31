using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.CancelInvoice;

public class CancelInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IStringLocalizer<Messages> localizer
) : IRequestHandler<CancelInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithItemsAsync(request.InvoiceId)
            ?? throw new NotFoundException(localizer[LocalizationKeys.InvoiceMessages.NotFound]);

        var clinicId = currentUserService.CurrentClinicId;
        if (clinicId != null && invoice.ClinicId != clinicId.Value)
            throw new ForbiddenException(localizer[LocalizationKeys.ExceptionMessages.Forbidden]);

        if (invoice.Status is not (InvoiceStatus.Draft or InvoiceStatus.Issued or InvoiceStatus.Paid))
            throw new BadRequestException(localizer[LocalizationKeys.InvoiceMessages.InvalidStatus]);

        invoice.Cancel(request.Reason);
        unitOfWork.InvoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<InvoiceDto>(invoice);
    }
}
