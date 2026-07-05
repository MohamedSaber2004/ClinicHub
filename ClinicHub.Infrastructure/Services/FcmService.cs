using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Domain.Repositories.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace ClinicHub.Infrastructure.Services
{
    public class FcmService : IFcmService
    {
        private readonly IUserFbTokenRepository _tokenRepository;
        private readonly INotificationBuilder _notificationBuilder;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseSettings _settings;
        private static readonly object _lock = new();
        private static bool _initialized;

        public FcmService(
            IUserFbTokenRepository tokenRepository,
            INotificationBuilder notificationBuilder,
            IUnitOfWork unitOfWork,
            IOptions<FirebaseSettings> settings)
        {
            _tokenRepository = tokenRepository;
            _notificationBuilder = notificationBuilder;
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            if (_initialized) return;

            lock (_lock)
            {
                if (_initialized) return;

                if (FirebaseApp.DefaultInstance is null)
                {
                    var credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settings.CredentialsFilePath);
                    if (!File.Exists(credentialPath))
                        credentialPath = _settings.CredentialsFilePath;

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credentialPath)
                    });
                }

                _initialized = true;
            }
        }

        public async Task SendToUserAsync(Guid userId, NotificationType type, Dictionary<string, object>? parameters = null)
        {
            var payload = await _notificationBuilder.BuildAsync(type, userId, parameters);
            var tokens = await _tokenRepository.GetUserTokensAsync(userId);

            foreach (var token in tokens)
            {
                try
                {
                    await SendToDeviceAsync(token.Token, payload, token.DevicePlatform);
                }
                catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                    ex.MessagingErrorCode == MessagingErrorCode.SenderIdMismatch)
                {
                    _tokenRepository.Delete(token);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task SendToDeviceAsync(string deviceToken, NotificationPayload payload, DevicePlatform platform)
        {
            var message = new FirebaseAdmin.Messaging.Message
            {
                Token = deviceToken,
                Data = payload.Data,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = payload.Title,
                    Body = payload.Body
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = _settings.Ios?.Sound,
                        Badge = _settings.Ios?.Badge,
                        ContentAvailable = true,
                        Category = _settings.Ios?.Category
                    }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = _settings.Android?.ChannelId,
                        Priority = NotificationPriority.HIGH,
                        Sound = _settings.Android?.Sound,
                        Icon = _settings.Android?.Icon
                    }
                },
                Webpush = new WebpushConfig
                {
                    Notification = new WebpushNotification
                    {
                        Title = payload.Title,
                        Body = payload.Body,
                        Icon = _settings.Web?.Icon
                    },
                    FcmOptions = (!string.IsNullOrWhiteSpace(_settings.Web?.Link) && 
                                  Uri.TryCreate(_settings.Web.Link, UriKind.Absolute, out var uri) && 
                                  uri.Scheme == Uri.UriSchemeHttps)
                        ? new WebpushFcmOptions { Link = _settings.Web.Link }
                        : null
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public async Task RegisterTokenAsync(Guid userId, string token, DevicePlatform platform)
        {
            var existing = await _tokenRepository.GetByTokenAsync(token);

            if (existing is not null)
            {
                if (existing.UserId != userId)
                {
                    _tokenRepository.Delete(existing);
                    var newToken = UserFbToken.Create(userId, token, platform);
                    await _tokenRepository.AddAsync(newToken);
                }
            }
            else
            {
                var oldTokens = await _tokenRepository.GetUserTokensByPlatformAsync(userId, platform);
                foreach (var old in oldTokens)
                    _tokenRepository.Delete(old);

                var userToken = UserFbToken.Create(userId, token, platform);
                await _tokenRepository.AddAsync(userToken);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnregisterTokenAsync(string token)
        {
            var existing = await _tokenRepository.GetByTokenAsync(token);
            if (existing is not null)
            {
                _tokenRepository.Delete(existing);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
