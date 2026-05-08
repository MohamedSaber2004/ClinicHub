using AutoMapper;
using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Conversations
{
    public class ConversationProfile : Profile
    {
        public ConversationProfile()
        {
            CreateMap<Message, MessageDto>();
            CreateMap<Conversation, ConversationDto>();
            CreateMap<Conversation, ConversationDetailDto>();
        }
    }
}
