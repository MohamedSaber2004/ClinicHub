using ClinicHub.Application.Features.Invoices.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Commands.CreateDraftInvoice;

public class CreateDraftInvoiceCommand : IRequest<InvoiceDto>
{
    public Guid? PatientId { get; set; }
    public List<CreateInvoiceLineItem> Items { get; set; } = [];
    public Domain.Enums.DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TaxRate { get; set; }
}
