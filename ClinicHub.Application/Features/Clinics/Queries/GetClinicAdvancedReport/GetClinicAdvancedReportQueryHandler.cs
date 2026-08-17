using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicAdvancedReport
{
    public class GetClinicAdvancedReportQueryHandler : IRequestHandler<GetClinicAdvancedReportQuery, ClinicAdvancedReportDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetClinicAdvancedReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<ClinicAdvancedReportDto> Handle(GetClinicAdvancedReportQuery request, CancellationToken cancellationToken)
        {
            var clinicId = _currentUserService.CurrentClinicId;
            if (clinicId == null)
                return new ClinicAdvancedReportDto();

            var requestedFrom = request.From?.Date;
            var requestedTo = request.To?.Date;

            var from = requestedFrom ?? DateTime.MinValue;
            var toExclusive = (requestedTo ?? DateTime.Now).Date.AddDays(1);

            var appointmentsQuery = _unitOfWork.AppointmentRepository
                .GetAllAsync(a => a.ClinicId == clinicId && !a.IsDeleted
                    && a.AppointmentDate >= from && a.AppointmentDate < toExclusive);

            var paymentsQuery = _unitOfWork.PaymentRepository
                .GetAllAsync(p => p.ClinicId == clinicId && p.Status == PaymentStatus.Paid
                    && p.PaidAt >= from && p.PaidAt < toExclusive);

            var totalAppointments = await appointmentsQuery.CountAsync(cancellationToken);
            var totalVisits = await appointmentsQuery
                .CountAsync(a => a.Status == AppointmentStatus.Completed, cancellationToken);
            var totalRevenue = await paymentsQuery
                .SumAsync(p => (double?)p.Amount, cancellationToken) ?? 0;

            var paidAppointmentCount = await paymentsQuery.CountAsync(p => p.AppointmentId != null, cancellationToken);

            var firstAppointmentDate = requestedFrom.HasValue
                ? requestedFrom.Value
                : totalAppointments > 0
                    ? await appointmentsQuery.MinAsync(a => a.AppointmentDate, cancellationToken)
                    : DateTime.Now.Date;

            var revenueByDoctor = await (
                from p in paymentsQuery
                join a in appointmentsQuery on p.AppointmentId equals a.Id
                group p by a.DoctorId into g
                select new { DoctorId = g.Key, Revenue = g.Sum(p => (double)p.Amount) })
                .ToListAsync(cancellationToken);

            var doctorNames = await _unitOfWork.DoctorRepository
                .GetAllAsync(d => revenueByDoctor.Select(r => r.DoctorId).Contains(d.Id))
                .Include(d => d.User)
                .Select(d => new { d.Id, Name = d.User.FullName })
                .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

            var appointmentsByStatus = await appointmentsQuery
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var busiestDays = await appointmentsQuery
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync(cancellationToken);

            return new ClinicAdvancedReportDto
            {
                From = firstAppointmentDate,
                To = requestedTo ?? DateTime.Now.Date,
                TotalAppointments = totalAppointments,
                TotalVisits = totalVisits,
                CompletionRate = totalAppointments == 0 ? 0 : Math.Round(totalVisits * 100.0 / totalAppointments, 2),
                TotalRevenue = Math.Round(totalRevenue, 2),
                AverageAppointmentValue = paidAppointmentCount == 0 ? 0 : Math.Round(totalRevenue / paidAppointmentCount, 2),
                RevenueByDoctor = revenueByDoctor
                    .Select(r => new DoctorRevenueDto
                    {
                        DoctorId = r.DoctorId,
                        DoctorName = doctorNames.GetValueOrDefault(r.DoctorId, "Unknown"),
                        Revenue = Math.Round(r.Revenue, 2)
                    })
                    .OrderByDescending(r => r.Revenue)
                    .ToList(),
                AppointmentsByStatus = appointmentsByStatus.ToDictionary(
                    x => x.Status.ToString(),
                    x => x.Count),
                BusiestDays = busiestDays
                    .Select(d => new BusiestDayDto { Date = d.Date, AppointmentCount = d.Count })
                    .ToList()
            };
        }
    }
}