using ClinicHub.Domain.Common;
using ClinicHub.Domain.Common.Interfaces;
using ClinicHub.Domain.Enums;

namespace ClinicHub.Domain.Entities;

public class Invoice : BaseEntity<Guid>, IClinicScopedEntity
{
    private readonly List<InvoiceItem> _items = [];

    public Guid ClinicId { get; private set; }
    Guid? IClinicScopedEntity.ClinicId => ClinicId;
    public Clinic Clinic { get; private set; } = null!;
    public Guid? PatientId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public decimal SubTotal { get; private set; }
    public DiscountType DiscountType { get; private set; }
    public decimal DiscountValue { get; private set; }
    public decimal TaxRate { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? IssuedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { }

    public Invoice(Guid clinicId, Guid? patientId)
    {
        ClinicId = clinicId;
        PatientId = patientId;
    }

    public void AddItem(InvoiceItem item)
    {
        _items.Add(item);
        RecalculateTotals();
    }

    public void RemoveItem(InvoiceItem item)
    {
        _items.Remove(item);
        RecalculateTotals();
    }

    public void ClearItems()
    {
        _items.Clear();
        RecalculateTotals();
    }

    public void SetLineItems(IEnumerable<InvoiceItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        RecalculateTotals();
    }

    public void SetDiscount(DiscountType type, decimal value)
    {
        DiscountType = type;
        DiscountValue = value;
        RecalculateTotals();
    }

    public void SetTaxRate(decimal taxRate)
    {
        TaxRate = taxRate;
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        SubTotal = _items.Sum(i => i.Total);

        var discountAmount = DiscountType == DiscountType.Percentage
            ? SubTotal * (DiscountValue / 100m)
            : DiscountValue;

        var afterDiscount = SubTotal - discountAmount;
        TaxAmount = afterDiscount * (TaxRate / 100m);
        Total = afterDiscount + TaxAmount;
    }

    public void Issue(string invoiceNumber)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only draft invoices can be issued.");

        InvoiceNumber = invoiceNumber;
        Status = InvoiceStatus.Issued;
        IssuedAt = DateTime.UtcNow;
    }

    public void MarkAsPaid()
    {
        if (Status != InvoiceStatus.Issued)
            throw new InvalidOperationException("Only issued invoices can be marked as paid.");

        Status = InvoiceStatus.Paid;
        PaidAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason)
    {
        if (Status is InvoiceStatus.Draft or InvoiceStatus.Issued or InvoiceStatus.Paid)
        {
            Status = InvoiceStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            CancellationReason = reason;
        }
        else
        {
            throw new InvalidOperationException("Invoice cannot be cancelled in its current status.");
        }
    }

    public void MarkAsRefunded()
    {
        if (Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Only paid or cancelled invoices can be refunded.");

        Status = InvoiceStatus.Refunded;
    }

    public void UpdatePatientInfo(Guid? patientId)
    {
        PatientId = patientId;
    }
}
