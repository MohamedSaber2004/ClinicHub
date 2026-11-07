using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ClinicHub.Application.Features.Admin.Commands.SendTestPushNotification
{
    public class SendTestPushCommandHandler : IRequestHandler<SendTestPushCommand, TestPushResultDto>
    {
        private const int PreviewLength = 12;

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFcmService _fcmService;
        private readonly IStringLocalizer<Messages> _localizer;

        public SendTestPushCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IFcmService fcmService,
            IStringLocalizer<Messages> localizer)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _fcmService = fcmService;
            _localizer = localizer;
        }

        public async Task<TestPushResultDto> Handle(SendTestPushCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            var title = string.IsNullOrWhiteSpace(request.Title)
                ? _localizer[LocalizationKeys.NotificationMessages.TestPushTitle.Value]
                : request.Title;

            var body = string.IsNullOrWhiteSpace(request.Body)
                ? _localizer[LocalizationKeys.NotificationMessages.TestPushBody.Value]
                : request.Body;

            var tokens = await _unitOfWork.UserFbTokenRepository.GetUserTokensAsync(request.UserId);

            var devices = new List<TestPushDeviceDto>();

            foreach (var token in tokens)
            {
                var report = await _fcmService.SendTestToTokenAsync(token.Token, title, body);

                devices.Add(new TestPushDeviceDto
                {
                    Platform = token.DevicePlatform,
                    TokenPreview = Preview(token.Token),
                    RegisteredAt = token.CreatedAt,
                    Success = report.Success,
                    MessageId = report.MessageId,
                    ErrorCode = report.ErrorCode,
                    Error = report.Error
                });
            }

            return new TestPushResultDto
            {
                UserId = request.UserId,
                FullName = user?.FullName ?? string.Empty,
                TotalDevices = devices.Count,
                DeliveredCount = devices.Count(d => d.Success),
                Devices = devices
            };
        }

        private static string Preview(string token)
            => token.Length > PreviewLength ? $"{token[..PreviewLength]}…" : token;
    }
}
