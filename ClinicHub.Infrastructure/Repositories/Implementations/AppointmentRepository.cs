using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.Repositories.Implementations.Base;
using ClinicHub.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Infrastructure.Repositories.Implementations
{
    public class AppointmentRepository : GenericRepository<Appointment, Guid>, IAppointmentRepository
    {
        private readonly ClinicHubContext _context;

        public AppointmentRepository(ClinicHubContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> HasOverlappingAppointmentAsync(Guid doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            return await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                               a.AppointmentDate == date.Date &&
                               a.Status != AppointmentStatus.Cancelled &&
                               a.StartTime < endTime && a.EndTime > startTime);
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAndDateAsync(Guid doctorId, DateTime date)
        {
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate == date.Date &&
                            a.Status != AppointmentStatus.Cancelled)
                .ToListAsync();
        }

        public async Task<(List<Appointment> items, int totalCount)> GetAppointmentsWithFiltersAsync(
            int pageNumber,
            int pageSize,
            Guid? doctorId = null,
            Guid? clinicId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            AppointmentStatus? status = null,
            string? patientName = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (doctorId.HasValue)
                query = query.Where(a => a.DoctorId == doctorId.Value);

            if (clinicId.HasValue)
                query = query.Where(a => a.ClinicId == clinicId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= endDate.Value.Date);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (!string.IsNullOrWhiteSpace(patientName))
                query = query.Where(a => a.PatientFullName.Contains(patientName));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
