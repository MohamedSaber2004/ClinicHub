using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.UpdateDraftInvoice;

public class UpdateDraftInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IStringLocalizer<Messages> localizer
) : IRequestHandler<UpdateDraftInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(UpdateDraftInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithItemsAsync(request.InvoiceId)
            ?? throw new NotFoundException(localizer[LocalizationKeys.InvoiceMessages.NotFound]);

        var clinicId = currentUserService.CurrentClinicId;
        if (clinicId != null && invoice.ClinicId != clinicId.Value)
            throw new Common.Exceptions.ForbiddenException(localizer[LocalizationKeys.ExceptionMessages.Forbidden]);

        if (invoice.Status != InvoiceStatus.Draft)
            throw new Common.Exceptions.BadRequestException(localizer[LocalizationKeys.InvoiceMessages.CannotModifyIssued]);

        invoice.UpdatePatientInfo(request.PatientId);

        var updatedItems = request.Items.Select(item => new InvoiceItem(
            invoice.Id,
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.Discount ?? 0,
            0));

        invoice.SetLineItems(updatedItems);
        invoice.SetDiscount(request.DiscountType, request.DiscountValue);
        invoice.SetTaxRate(request.TaxRate);

        unitOfWork.InvoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<InvoiceDto>(invoice);
    }
}
