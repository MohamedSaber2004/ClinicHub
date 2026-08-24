using ClinicHub.Application.Features.PlatformSettings.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.PlatformSettings.Commands.UpdatePlatformSetting
{
    public class UpdatePlatformSettingCommandHandler : IRequestHandler<UpdatePlatformSettingCommand, PlatformSettingDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePlatformSettingCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PlatformSettingDto> Handle(UpdatePlatformSettingCommand request, CancellationToken cancellationToken)
        {
            if (request.AppointmentFeePercent < 0 || request.AppointmentFeePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(request.AppointmentFeePercent), "Fee percentage must be between 0 and 100.");

            var repository = _unitOfWork.GetRepository<PlatformSetting, Guid>();
            var setting = await repository.GetAllAsync(s => !s.IsDeleted)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting == null)
            {
                setting = new PlatformSetting(request.AppointmentFeePercent);
                await repository.AddAsync(setting);
            }
            else
            {
                setting.UpdateAppointmentFeePercent(request.AppointmentFeePercent, request.UpdatedBy);
            }

            await _unitOfWork.SaveChangesAsync();

            return new PlatformSettingDto
            {
                AppointmentFeePercent = setting.AppointmentFeePercent
            };
        }
    }
}
