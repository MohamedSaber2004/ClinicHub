using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Options;
using Microsoft.Extensions.Options;
using PusherServer;

namespace ClinicHub.Infrastructure.Services
{
    public class PusherService : IPusherService
    {
        private readonly Pusher _pusher;

        public PusherService(IOptions<PusherSettings> options)
        {
            var settings = options.Value;
            var pusherOptions = new PusherOptions
            {
                Cluster = settings.Cluster,
                Encrypted = true
            };

            _pusher = new Pusher(
                settings.AppId,
                settings.AppKey,
                settings.AppSecret,
                pusherOptions);
        }

        public async Task TriggerEventAsync(string channel, string @event, object data)
        {
            try
            {
                await _pusher.TriggerAsync(channel, @event, data);
            }
            catch (System.Exception ex)
            {
                // In a real app, use ILogger. For now, we'll assume Serilog or similar is capturing this.
                System.Console.WriteLine($"Pusher Error: {ex.Message}");
                throw;
            }
        }

        public string Authenticate(string channelName, string socketId, string userId, object? userInfo = null)
        {
            if (string.IsNullOrEmpty(socketId)) throw new System.ArgumentNullException(nameof(socketId));

            if (channelName.StartsWith("presence-"))
            {
                var presenceData = new PresenceChannelData
                {
                    user_id = userId,
                    user_info = userInfo
                };
                return _pusher.Authenticate(channelName, socketId, presenceData).ToJson();
            }

            return _pusher.Authenticate(channelName, socketId).ToJson();
        }
    }
}
