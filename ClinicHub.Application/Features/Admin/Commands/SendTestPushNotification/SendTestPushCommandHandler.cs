using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Admin.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Commands.SendTestPushNotification
{
    public class SendTestPushCommandHandler : IRequestHandler<SendTestPushCommand, TestPushResultDto>
    {
        private const int TokenPreviewLength = 12;

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationBuilder _notificationBuilder;
        private readonly IFcmService _fcmService;

        public SendTestPushCommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            INotificationBuilder notificationBuilder,
            IFcmService fcmService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _notificationBuilder = notificationBuilder;
            _fcmService = fcmService;
        }

        public async Task<TestPushResultDto> Handle(SendTestPushCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            var parameters = await BuildParametersFromLatestAppointmentAsync(request.UserId, cancellationToken);
            var payload = await _notificationBuilder.BuildAsync(request.Type, request.UserId, parameters);

            var tokens = await _unitOfWork.UserFbTokenRepository.GetUserTokensAsync(request.UserId);

            var devices = new List<TestPushDeviceDto>();

            foreach (var token in tokens)
            {
                var report = await _fcmService.SendToDeviceWithReportAsync(token.Token, payload, token.DevicePlatform);

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

            await _unitOfWork.SaveChangesAsync();

            return new TestPushResultDto
            {
                UserId = request.UserId,
                FullName = user?.FullName ?? string.Empty,
                Type = payload.Type,
                Title = payload.Title,
                Body = payload.Body,
                Link = payload.Link,
                Badge = payload.Badge,
                TotalDevices = devices.Count,
                DeliveredCount = devices.Count(d => d.Success),
                Devices = devices
            };
        }

        private async Task<Dictionary<string, object>> BuildParametersFromLatestAppointmentAsync(Guid userId, CancellationToken cancellationToken)
        {
            var appointment = await _unitOfWork.AppointmentRepository
                .GetAllWithIncluding(a => a.BookedByUserId == userId, a => a.Clinic)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (appointment is null)
                return [];

            return new Dictionary<string, object>
            {
                ["clinicName"] = appointment.Clinic?.Name ?? string.Empty,
                ["date"] = appointment.AppointmentDate.ToString("yyyy-MM-dd"),
                ["time"] = appointment.StartTime.ToString(@"hh\:mm"),
                ["appointmentId"] = appointment.Id.ToString()
            };
        }

        private static string Preview(string token)
            => token.Length > TokenPreviewLength ? $"{token[..TokenPreviewLength]}…" : token;
    }
}
