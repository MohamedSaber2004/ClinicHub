using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.SetTyping
{
    public class SetTypingCommand : IRequest<bool>
    {
        public Guid ConversationId { get; set; }
        public bool IsTyping { get; set; }
    }
}
