using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Invoices.DTOs;

public class UpdateInvoiceRequest
{
    public Guid? PatientId { get; set; }
    public List<UpdateInvoiceLineItem> Items { get; set; } = [];
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TaxRate { get; set; }
}

public class UpdateInvoiceLineItem
{
    public Guid? Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal? Discount { get; set; }
}
