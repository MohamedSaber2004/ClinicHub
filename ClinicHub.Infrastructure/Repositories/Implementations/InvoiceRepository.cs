using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations;

public class InvoiceRepository : GenericRepository<Invoice, Guid>, IInvoiceRepository
{
    private readonly ClinicHubContext _context;

    public InvoiceRepository(ClinicHubContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(List<Invoice> items, int totalCount)> GetByClinicIdPaginatedAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        InvoiceStatus? statusFilter,
        DateTime? fromDate,
        DateTime? toDate,
        Guid? patientId)
    {
        var query = _context.Invoices
            .Include(i => i.Items)
            .AsQueryable();

        query = query.Where(i => i.ClinicId == clinicId);

        if (statusFilter.HasValue)
            query = query.Where(i => i.Status == statusFilter.Value);

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAt <= toDate.Value);

        if (patientId.HasValue)
            query = query.Where(i => i.PatientId == patientId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(decimal todayRevenue, int paidCount, int pendingCount, int draftCount, int cancelledCount)> GetInvoiceStatsAsync(Guid clinicId)
    {
        var today = DateTime.UtcNow.Date;

        var todayRevenue = await _context.Invoices
            .Where(i => i.ClinicId == clinicId && i.Status == InvoiceStatus.Paid && i.PaidAt != null && i.PaidAt.Value.Date == today)
            .SumAsync(i => i.Total);

        var paidCount = await _context.Invoices
            .CountAsync(i => i.ClinicId == clinicId && i.Status == InvoiceStatus.Paid);

        var pendingCount = await _context.Invoices
            .CountAsync(i => i.ClinicId == clinicId && i.Status == InvoiceStatus.Issued);

        var draftCount = await _context.Invoices
            .CountAsync(i => i.ClinicId == clinicId && i.Status == InvoiceStatus.Draft);

        var cancelledCount = await _context.Invoices
            .CountAsync(i => i.ClinicId == clinicId && i.Status == InvoiceStatus.Cancelled);

        return (todayRevenue, paidCount, pendingCount, draftCount, cancelledCount);
    }

    public async Task<Invoice?> GetByIdWithItemsAsync(Guid id)
    {
        return await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
}
