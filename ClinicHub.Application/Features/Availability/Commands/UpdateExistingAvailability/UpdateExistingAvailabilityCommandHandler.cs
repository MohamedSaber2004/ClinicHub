using AutoMapper;
using ClinicHub.Application.Features.Availability.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Availability.Commands.UpdateExistingAvailability
{
    public class UpdateExistingAvailabilityCommandHandler : IRequestHandler<UpdateExistingAvailabilityCommand, AvailabilityDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateExistingAvailabilityCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AvailabilityDto> Handle(UpdateExistingAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.DoctorAvailabilityRepository;
            var availability = await repo.GetByIdAsync(request.Id);

            availability.Update(request.DayOfWeek!.Value, request.StartTime!.Value, request.EndTime!.Value, request.SlotDurationMinutes!.Value);

            repo.Update(availability);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AvailabilityDto>(availability);
        }
    }
}
