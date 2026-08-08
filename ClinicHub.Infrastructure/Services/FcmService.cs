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
                catch (Exception)
                {
                    // Ignore FCM dispatch failures (e.g. invalid credentials or network issues) so approval operation succeeds
                }
            }

            // NOTE: No SaveChanges here on purpose. The notification row (and any deleted
            // unregistered tokens) stay tracked on the shared unit of work and are committed
            // by a single SaveChangesAsync at the end of the calling operation (handler/job),
            // so each request performs exactly one DB write transaction.
        }

        public async Task SendToDeviceAsync(string deviceToken, NotificationPayload payload, DevicePlatform platform)
        {
            var hasValidLink = !string.IsNullOrWhiteSpace(payload.Link) &&
                               Uri.TryCreate(payload.Link, UriKind.Absolute, out var linkUri) &&
                               linkUri.Scheme is "https" or "http";

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
                        Sound = _settings.Ios?.Sound ?? "default",
                        Badge = _settings.Ios?.Badge ?? 1,
                        ContentAvailable = true,
                        Category = _settings.Ios?.Category
                    }
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = _settings.Android?.ChannelId ?? "clinic_hub_default",
                        Priority = NotificationPriority.HIGH,
                        Sound = _settings.Android?.Sound ?? "default",
                        //Icon = _settings.Android?.Icon ?? "notification_logo",
                        //ImageUrl = _settings.Android?.ImageUrl
                    }
                },
                Webpush = hasValidLink
                    ? new WebpushConfig
                    {
                        Notification = new WebpushNotification
                        {
                            Title = payload.Title,
                            Body = payload.Body,
                            //Icon = _settings.Web?.Icon ?? "/notification_logo.png"
                        },
                        FcmOptions = new WebpushFcmOptions { Link = payload.Link! }
                    }
                    : new WebpushConfig
                    {
                        Notification = new WebpushNotification
                        {
                            Title = payload.Title,
                            Body = payload.Body,
                            //Icon = _settings.Web?.Icon ?? "/notification_logo.png"
                        }
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
