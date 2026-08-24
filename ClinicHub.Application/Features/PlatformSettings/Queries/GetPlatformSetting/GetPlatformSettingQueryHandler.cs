using ClinicHub.Application.Features.PlatformSettings.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.PlatformSettings.Queries.GetPlatformSetting
{
    public class GetPlatformSettingQueryHandler : IRequestHandler<GetPlatformSettingQuery, PlatformSettingDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPlatformSettingQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PlatformSettingDto> Handle(GetPlatformSettingQuery request, CancellationToken cancellationToken)
        {
            var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
                .GetAllAsync(s => !s.IsDeleted)
                .OrderBy(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return new PlatformSettingDto
            {
                AppointmentFeePercent = setting?.AppointmentFeePercent ?? 0m
            };
        }
    }
}
