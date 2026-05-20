using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.RealTime.Commands.SetActiveConversation
{
    public class SetActiveConversationCommand : IRequest<bool>
    {
        public Guid? ConversationId { get; set; }
    }
}
