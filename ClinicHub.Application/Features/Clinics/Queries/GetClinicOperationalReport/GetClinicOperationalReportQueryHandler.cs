using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicOperationalReport
{
    public class GetClinicOperationalReportQueryHandler
        : IRequestHandler<GetClinicOperationalReportQuery, ClinicOperationalReportDto>
    {
        private static readonly AppointmentStatus[] CancelledLike =
        {
            AppointmentStatus.Cancelled,
            AppointmentStatus.Rejected
        };

        private static readonly string[] DayNamesAr =
        {
            "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"
        };

        private static readonly (int FromHour, int ToHour, string Label)[] Slots =
        {
            (0, 8, "12 ص - 8 ص"),
            (8, 12, "8 ص - 12 م"),
            (12, 16, "12 م - 4 م"),
            (16, 20, "4 م - 8 م"),
            (20, 24, "8 م - 12 ص")
        };

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetClinicOperationalReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicOperationalReportDto> Handle(GetClinicOperationalReportQuery request, CancellationToken cancellationToken)
        {
            var result = new ClinicOperationalReportDto();

            if (_currentUserService.CurrentClinicId is null)
                return result;

            var clinicId = _currentUserService.CurrentClinicId.Value;

            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var (from, to) = request.Period?.ToLower() switch
            {
                "today" => (today, tomorrow),
                "month" => (new DateTime(today.Year, today.Month, 1), tomorrow),
                "last30" => (today.AddDays(-29), tomorrow),
                _ => (today.AddDays(-6), tomorrow)
            };

            result.From = from;
            result.To = to.AddDays(-1);

            var appointmentsQuery = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.ClinicId == clinicId
                    && !a.IsDeleted
                    && a.AppointmentDate >= from
                    && a.AppointmentDate < to);

            if (request.DoctorId.HasValue)
                appointmentsQuery = appointmentsQuery.Where(a => a.DoctorId == request.DoctorId.Value);

            var appointments = await appointmentsQuery
                .Select(a => new { a.Id, a.AppointmentDate, a.StartTime, a.Status, a.PatientFullName, a.DoctorId })
                .ToListAsync(cancellationToken);

            result.TotalAppointments = appointments.Count;
            result.CompletedVisits = appointments.Count(a => a.Status == AppointmentStatus.Completed);
            result.CancelledVisits = appointments.Count(a => CancelledLike.Contains(a.Status));
            result.NoShowVisits = appointments.Count(a => a.Status == AppointmentStatus.NoShow);
            result.CompletionRate = Percentage(result.CompletedVisits, result.TotalAppointments);
            result.CancellationRate = Percentage(result.CancelledVisits, result.TotalAppointments);
            result.NoShowRate = Percentage(result.NoShowVisits, result.TotalAppointments);

            var paidPaymentsQuery = _unitOfWork.PaymentRepository
                .GetAllAsync(p => p.ClinicId == clinicId
                    && p.Type == PaymentType.Appointment
                    && p.Status == PaymentStatus.Paid
                    && p.PaidAt != null
                    && p.PaidAt >= from
                    && p.PaidAt < to);

            result.PeriodRevenue = await paidPaymentsQuery.SumAsync(p => (double?)p.Amount, cancellationToken) ?? 0;

            // Hourly traffic buckets.
            var slotGroups = appointments
                .GroupBy(a => SlotIndex(a.StartTime.Hours))
                .ToDictionary(g => g.Key, g => g.Count());

            var peakSlotIndex = slotGroups.Count == 0
                ? -1
                : slotGroups.OrderByDescending(kv => kv.Value).First().Key;

            for (var i = 0; i < Slots.Length; i++)
            {
                slotGroups.TryGetValue(i, out var count);
                result.HourlyTraffic.Add(new OperationalHourSlotDto
                {
                    SlotLabel = Slots[i].Label,
                    AppointmentCount = count,
                    IsPeak = i == peakSlotIndex && count > 0,
                    HeightPercentage = count == 0 ? 0 : Math.Max(15, (int)Math.Round(count * 100.0 / slotGroups.Values.Max()))
                });
            }

            if (peakSlotIndex >= 0)
                result.PeakTimeSlot = Slots[peakSlotIndex].Label;

            // Weekday workload across the period.
            var dayGroups = appointments
                .GroupBy(a => a.AppointmentDate.DayOfWeek)
                .Select(g => new
                {
                    Day = g.Key,
                    Total = g.Count(),
                    Completed = g.Count(x => x.Status == AppointmentStatus.Completed)
                })
                .ToList();

            var maxDayTotal = dayGroups.Count == 0 ? 0 : dayGroups.Max(d => d.Total);
            foreach (var g in dayGroups.OrderByDescending(d => (int)d.Day == 6 ? 0 : (int)d.Day + 1))
            {
                result.WeeklyWorkload.Add(new OperationalDayLoadDto
                {
                    DayName = DayNamesAr[(int)g.Day],
                    TotalAppointments = g.Total,
                    CompletedVisits = g.Completed,
                    CapacityPercentage = maxDayTotal == 0 ? 0 : (int)Math.Round(g.Total * 100.0 / maxDayTotal)
                });
            }

            var busiest = dayGroups.OrderByDescending(d => d.Total).FirstOrDefault();
            if (busiest != null && busiest.Total > 0)
                result.BusiestDayName = DayNamesAr[(int)busiest.Day];

            // Doctor operational stats for the clinic's doctors.
            var doctors = await _unitOfWork.GetRepository<Doctor, Guid>()
                .GetAllAsync(d => d.ClinicId == clinicId && !d.IsDeleted)
                .Select(d => new
                {
                    d.Id,
                    Name = d.User.FullName,
                    Specialty = d.Specialization.Name
                })
                .ToListAsync(cancellationToken);

            if (request.DoctorId.HasValue)
                doctors = doctors.Where(d => d.Id == request.DoctorId.Value).ToList();

            var doctorIds = doctors.Select(d => d.Id).ToList();

            var ratings = await _unitOfWork.GetRepository<Rating, Guid>()
                .GetAllAsync(r => r.Type == RatingType.Doctor
                    && r.DoctorId != null
                    && doctorIds.Contains(r.DoctorId.Value)
                    && !r.IsDeleted)
                .GroupBy(r => r.DoctorId!.Value)
                .Select(g => new { DoctorId = g.Key, Avg = g.Average(x => (double)x.Value), Count = g.Count() })
                .ToListAsync(cancellationToken);

            var ratingsLookup = ratings.ToDictionary(r => r.DoctorId);

            foreach (var doctor in doctors)
            {
                var doctorAppointments = appointments.Where(a => a.DoctorId == doctor.Id).ToList();
                var completed = doctorAppointments.Count(a => a.Status == AppointmentStatus.Completed);
                var cancelled = doctorAppointments.Count(a => CancelledLike.Contains(a.Status));
                var noShow = doctorAppointments.Count(a => a.Status == AppointmentStatus.NoShow);
                ratingsLookup.TryGetValue(doctor.Id, out var rating);

                result.Doctors.Add(new OperationalDoctorStatDto
                {
                    DoctorId = doctor.Id,
                    Name = doctor.Name,
                    Specialty = doctor.Specialty ?? "",
                    TotalAppointments = doctorAppointments.Count,
                    CompletedVisits = completed,
                    CancelledCount = cancelled,
                    NoShowCount = noShow,
                    CompletionPercentage = Percentage(completed, doctorAppointments.Count),
                    Rating = rating?.Avg ?? 0,
                    ReviewCount = rating?.Count ?? 0
                });
            }

            // Recent visit log (latest 15 in the period).
            result.RecentVisits = await appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Take(15)
                .Select(a => new OperationalVisitLogDto
                {
                    AppointmentId = a.Id,
                    PatientName = a.PatientFullName,
                    DoctorName = a.Doctor.User.FullName,
                    Specialty = a.Doctor.Specialization.Name,
                    AppointmentDate = a.AppointmentDate,
                    StartTime = a.StartTime,
                    Status = (int)a.Status
                })
                .ToListAsync(cancellationToken);

            return result;
        }

        private static int SlotIndex(int hour)
        {
            for (var i = 0; i < Slots.Length; i++)
            {
                if (hour >= Slots[i].FromHour && hour < Slots[i].ToHour)
                    return i;
            }
            return Slots.Length - 1;
        }

        private static double Percentage(int part, int total)
            => total == 0 ? 0 : Math.Round(part * 100.0 / total, 1);

        private static double Percentage(int part, double total)
            => total <= 0 ? 0 : Math.Round(part * 100.0 / total, 1);
    }
}
