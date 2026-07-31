using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Services;

public class InvoiceNumberService : IInvoiceNumberService
{
    private readonly ClinicHubContext _context;

    public InvoiceNumberService(ClinicHubContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateNextAsync(Guid clinicId, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INV-{year}-";

        var lastNumber = await _context.Invoices
            .Where(i => i.ClinicId == clinicId && i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSeq = 1;
        if (lastNumber != null && int.TryParse(lastNumber[prefix.Length..], out var lastSeq))
            nextSeq = lastSeq + 1;

        return $"{prefix}{nextSeq:D4}";
    }
}
