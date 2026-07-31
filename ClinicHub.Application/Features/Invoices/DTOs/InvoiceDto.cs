using ClinicHub.Domain.Enums;

namespace ClinicHub.Application.Features.Invoices.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public Guid? PatientId { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Total { get; set; }
    public List<InvoiceLineItemDto> LineItems { get; set; } = [];
    public List<PaymentSettlementDto> PaymentSettlements { get; set; } = [];
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}
