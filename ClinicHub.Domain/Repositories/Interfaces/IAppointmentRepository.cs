using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces.Base;
using System.Linq.Expressions;

namespace ClinicHub.Domain.Repositories.Interfaces
{
    public interface IAppointmentRepository : IGenericRepository<Appointment, Guid>
    {
        Task<bool> HasOverlappingAppointmentAsync(Guid doctorId, DateTime date, TimeSpan startTime, TimeSpan EndTime);
        Task<List<Appointment>> GetAppointmentsByDoctorAndDateAsync(Guid doctorId, DateTime date);

        Task<(List<Appointment> items, int totalCount)> GetAppointmentsWithFiltersAsync(
            int pageNumber,
            int pageSize,
            Guid? doctorId = null,
            Guid? clinicId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            AppointmentStatus? status = null,
            string? patientName = null);
    }
}
