using ClinicHub.Application.Features.PlatformSettings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.PlatformSettings.Commands.UpdatePlatformSetting
{
    public class UpdatePlatformSettingCommand : IRequest<PlatformSettingDto>
    {
        public decimal AppointmentFeePercent { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
