using ClinicHub.Application.Common.Models;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Appointments.Queries.GetAvailableSlots
{
    public class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, List<TimeSlotDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAvailableSlotsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TimeSlotDto>> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
        {
            var dayOfWeek = request.Date.DayOfWeek;

            var availabilities = await _unitOfWork.DoctorAvailabilityRepository
                .GetAllAsync(a => a.DoctorId == request.DoctorId && a.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            if (!availabilities.Any())
            {
                return new List<TimeSlotDto>();
            }

            var bookedAppointments = await _unitOfWork.AppointmentRepository
                .GetAppointmentsByDoctorAndDateAsync(request.DoctorId, request.Date);

            var timeSlots = new List<TimeSlotDto>();

            foreach (var availability in availabilities)
            {
                var slotDuration = TimeSpan.FromMinutes(availability.SlotDurationMinutes > 0 ? availability.SlotDurationMinutes : 30);
                var currentTime = availability.StartTime;

                while (currentTime + slotDuration <= availability.EndTime)
                {
                    var slotEndTime = currentTime + slotDuration;

                    var isBooked = bookedAppointments.Any(a => 
                        a.StartTime < slotEndTime && a.EndTime > currentTime);

                    timeSlots.Add(new TimeSlotDto
                    {
                        StartTime = currentTime,
                        EndTime = slotEndTime,
                        IsAvailable = !isBooked
                    });

                    currentTime = slotEndTime;
                }
            }

            return timeSlots.OrderBy(t => t.StartTime).ToList();
        }
    }
}
