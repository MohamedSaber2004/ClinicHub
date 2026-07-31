using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces;

public interface IInvoiceRepository : IGenericRepository<Invoice, Guid>
{
    Task<(List<Invoice> items, int totalCount)> GetByClinicIdPaginatedAsync(Guid clinicId, int pageNumber, int pageSize, InvoiceStatus? statusFilter, DateTime? fromDate, DateTime? toDate, Guid? patientId);
    Task<(decimal todayRevenue, int paidCount, int pendingCount, int draftCount, int cancelledCount)> GetInvoiceStatsAsync(Guid clinicId);
    Task<Invoice?> GetByIdWithItemsAsync(Guid id);
}
