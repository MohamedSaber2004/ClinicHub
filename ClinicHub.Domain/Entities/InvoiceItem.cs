using ClinicHub.Domain.Common;

namespace ClinicHub.Domain.Entities;

public class InvoiceItem : BaseEntity<Guid>
{
    public Guid InvoiceId { get; private set; }
    public Invoice Invoice { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal Total { get; private set; }

    private InvoiceItem() { }

    public InvoiceItem(Guid invoiceId, string description, int quantity, decimal unitPrice, decimal discount = 0, decimal taxRate = 0)
    {
        InvoiceId = invoiceId;
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
        TaxRate = taxRate;
        Recalculate();
    }

    public void Update(string description, int quantity, decimal unitPrice, decimal discount, decimal taxRate)
    {
        Description = description;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
        TaxRate = taxRate;
        Recalculate();
    }

    private void Recalculate()
    {
        var lineTotal = Quantity * UnitPrice;
        var discountAmount = lineTotal * (Discount / 100m);
        var afterDiscount = lineTotal - discountAmount;
        Total = afterDiscount * (1 + TaxRate / 100m);
    }
}
