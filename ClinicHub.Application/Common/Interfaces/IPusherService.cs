using System.Threading.Tasks;

namespace ClinicHub.Application.Common.Interfaces
{
    public interface IPusherService
    {
        Task TriggerEventAsync(string channel, string @event, object data);
        string Authenticate(string channelName, string socketId, string userId, object? userInfo = null);
    }
}
