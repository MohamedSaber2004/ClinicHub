using ClinicHub.Application.Features.Invoices.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Commands.UpdateDraftInvoice;

public class UpdateDraftInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid InvoiceId { get; set; }
    public Guid? PatientId { get; set; }
    public List<UpdateInvoiceLineItem> Items { get; set; } = [];
    public Domain.Enums.DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TaxRate { get; set; }
}
