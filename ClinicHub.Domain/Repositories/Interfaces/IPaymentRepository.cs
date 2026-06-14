using System;
using System.Threading.Tasks;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Repositories.Interfaces.Base;

namespace ClinicHub.Domain.Repositories.Interfaces;

public interface IPaymentRepository : IGenericRepository<Payment, Guid>
{
    Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId);
    Task<Payment?> GetByPaymobOrderIdAsync(string paymobOrderId);
}