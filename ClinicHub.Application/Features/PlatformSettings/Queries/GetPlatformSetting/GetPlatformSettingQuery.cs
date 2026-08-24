using ClinicHub.Application.Features.PlatformSettings.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.PlatformSettings.Queries.GetPlatformSetting
{
    public class GetPlatformSettingQuery : IRequest<PlatformSettingDto>
    {
    }
}
