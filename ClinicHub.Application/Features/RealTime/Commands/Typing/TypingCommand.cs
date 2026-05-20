using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.Typing
{
    public class TypingCommand : IRequest<bool>
    {
        public Guid ConversationId { get; set; }
        public bool IsTyping { get; set; }
    }
}
