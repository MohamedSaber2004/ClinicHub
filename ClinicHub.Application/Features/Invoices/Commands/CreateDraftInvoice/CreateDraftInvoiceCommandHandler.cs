using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Commands.CreateDraftInvoice;

public class CreateDraftInvoiceCommandHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService
) : IRequestHandler<CreateDraftInvoiceCommand, InvoiceDto>
{
    public async Task<InvoiceDto> Handle(CreateDraftInvoiceCommand request, CancellationToken cancellationToken)
    {
        var clinicId = currentUserService.CurrentClinicId
            ?? throw new InvalidOperationException("User must be associated with a clinic.");

        var invoice = new Invoice(clinicId, request.PatientId);

        foreach (var item in request.Items)
        {
            invoice.AddItem(new InvoiceItem(
                invoice.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.Discount ?? 0,
                0));
        }

        invoice.SetDiscount(request.DiscountType, request.DiscountValue);
        invoice.SetTaxRate(request.TaxRate);

        await unitOfWork.InvoiceRepository.AddAsync(invoice);
        await unitOfWork.SaveChangesAsync();

        return mapper.Map<InvoiceDto>(invoice);
    }
}
