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
            // Timezone-free: compare calendar dates only (Kind=Unspecified midnight).
            // A Pending request holds its slot until staff accepts, rejects, or
            // cancels it — there is no automatic expiry.
            var day = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            return await _context.Appointments
                .AnyAsync(a => a.DoctorId == doctorId &&
                               a.AppointmentDate == day &&
                               a.Status != AppointmentStatus.Cancelled &&
                               a.Status != AppointmentStatus.Rejected &&
                               a.StartTime < endTime && a.EndTime > startTime);
        }

        public async Task<List<Appointment>> GetAppointmentsByDoctorAndDateAsync(Guid doctorId, DateTime date)
        {
            var day = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified);
            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                            a.AppointmentDate == day &&
                            a.Status != AppointmentStatus.Cancelled &&
                            a.Status != AppointmentStatus.Rejected)
                .ToListAsync();
        }

        public async Task<(List<Appointment> items, int totalCount)> GetAppointmentsWithFiltersAsync(
            int pageNumber,
            int pageSize,
            Guid? doctorId = null,
            Guid? clinicId = null,
            string? startDate = null,
            string? endDate = null,
            AppointmentStatus? status = null,
            string? patientName = null,
            Guid? bookedByUserId = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (bookedByUserId.HasValue)
                query = query.Where(a => a.BookedByUserId == bookedByUserId.Value);

            if (doctorId.HasValue)
                query = query.Where(a => a.DoctorId == doctorId.Value);

            if (clinicId.HasValue)
                query = query.Where(a => a.ClinicId == clinicId.Value);

            if (!string.IsNullOrEmpty(startDate))
                query = query.Where(a => a.AppointmentDate >= DateTime.Parse(startDate));

            if (!string.IsNullOrEmpty(endDate))
                query = query.Where(a => a.AppointmentDate <= DateTime.Parse(endDate));
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
