using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations;

public class PaymentRepository : GenericRepository<Payment, Guid>, IPaymentRepository
{
    private readonly ClinicHubContext _context;

    public PaymentRepository(ClinicHubContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId);
    }

    public async Task<Payment?> GetByPaymobOrderIdAsync(string paymobOrderId)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymobOrderId == paymobOrderId);
    }
}