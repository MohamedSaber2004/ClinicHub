using ClinicHub.Application.Common.Options;
using ClinicHub.Application.Features.PlatformSettings.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicHub.Application.Features.PlatformSettings.Queries.GetPlatformSetting
{
    public class GetPlatformSettingQueryHandler : IRequestHandler<GetPlatformSettingQuery, PlatformSettingDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly PayoutSettings _payoutSettings;

        public GetPlatformSettingQueryHandler(IUnitOfWork unitOfWork, IOptions<PayoutSettings> payoutSettings)
        {
            _unitOfWork = unitOfWork;
            _payoutSettings = payoutSettings.Value;
        }

        public async Task<PlatformSettingDto> Handle(GetPlatformSettingQuery request, CancellationToken cancellationToken)
        {
            var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
                .GetAllAsync(s => !s.IsDeleted)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return new PlatformSettingDto
            {
                AppointmentFeePercent = setting?.AppointmentFeePercent ?? 0m,
                MinimumPayoutAmount = setting?.MinimumPayoutAmount ?? _payoutSettings.MinimumPayoutAmount
            };
        }
    }
}
