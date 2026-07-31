using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Invoices.DTOs;

public class CreateInvoiceRequest
{
    public Guid? PatientId { get; set; }
    public List<CreateInvoiceLineItem> Items { get; set; } = [];
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TaxRate { get; set; }
}

public class CreateInvoiceLineItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? Discount { get; set; }
}
