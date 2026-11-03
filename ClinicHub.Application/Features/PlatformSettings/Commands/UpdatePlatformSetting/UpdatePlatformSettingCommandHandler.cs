using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.PlatformSettings.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.PlatformSettings.Commands.UpdatePlatformSetting
{
    public class UpdatePlatformSettingCommandHandler : IRequestHandler<UpdatePlatformSettingCommand, PlatformSettingDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayoutSettings _payoutSettings;

        public UpdatePlatformSettingCommandHandler(IUnitOfWork unitOfWork, IOptions<PayoutSettings> payoutSettings)
        {
            _unitOfWork = unitOfWork;
            _payoutSettings = payoutSettings.Value;
        }

        public async Task<PlatformSettingDto> Handle(UpdatePlatformSettingCommand request, CancellationToken cancellationToken)
        {
            var repository = _unitOfWork.GetRepository<PlatformSetting, Guid>();
            var setting = await repository.GetAllAsync(s => !s.IsDeleted)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting == null)
            {
                setting = new PlatformSetting(
                    request.AppointmentFeePercent ?? 0m,
                    request.MinimumPayoutAmount ?? _payoutSettings.MinimumPayoutAmount);
                await repository.AddAsync(setting);
            }
            else
            {
                if (request.AppointmentFeePercent is decimal percent)
                    setting.UpdateAppointmentFeePercent(percent, request.UpdatedBy);

                if (request.MinimumPayoutAmount is decimal minimumAmount)
                    setting.UpdateMinimumPayoutAmount(minimumAmount, request.UpdatedBy);
            }

            await _unitOfWork.SaveChangesAsync();

            return new PlatformSettingDto
            {
                AppointmentFeePercent = setting.AppointmentFeePercent,
                MinimumPayoutAmount = setting.MinimumPayoutAmount
            };
        }
    }
}
