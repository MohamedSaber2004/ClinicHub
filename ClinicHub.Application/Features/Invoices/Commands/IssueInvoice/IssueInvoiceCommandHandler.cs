using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Invoices.Commands.IssueInvoice;

public class IssueInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService,
    IInvoiceNumberService invoiceNumberService,
    IStringLocalizer<Messages> localizer
) : IRequestHandler<IssueInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await unitOfWork.InvoiceRepository.GetByIdWithItemsAsync(request.InvoiceId)
            ?? throw new NotFoundException(localizer[LocalizationKeys.InvoiceMessages.NotFound]);

        var clinicId = currentUserService.CurrentClinicId;
        if (clinicId != null && invoice.ClinicId != clinicId.Value)
            throw new ForbiddenException(localizer[LocalizationKeys.ExceptionMessages.Forbidden]);

        if (invoice.Status != InvoiceStatus.Draft)
            throw new BadRequestException(localizer[LocalizationKeys.InvoiceMessages.AlreadyIssued]);

        var invoiceNumber = await invoiceNumberService.GenerateNextAsync(invoice.ClinicId, cancellationToken);
        invoice.Issue(invoiceNumber);

        unitOfWork.InvoiceRepository.Update(invoice);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<InvoiceDto>(invoice);
    }
}
