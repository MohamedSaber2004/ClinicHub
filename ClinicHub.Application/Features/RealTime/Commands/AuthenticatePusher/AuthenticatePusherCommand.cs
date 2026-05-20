using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.AuthenticatePusher
{
    public class AuthenticatePusherCommand : IRequest<string>
    {
        public string SocketId { get; set; } = null!;
        public string ChannelName { get; set; } = null!;
    }
}
