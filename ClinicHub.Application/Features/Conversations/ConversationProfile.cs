using AutoMapper;
using ClinicHub.Application.Features.Conversations.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Conversations
{
    public class ConversationProfile : Profile
    {
        public ConversationProfile()
        {
            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderName, opt => opt.Ignore())
                .ForMember(dest => dest.SenderProfilePictureUrl, opt => opt.Ignore())
                .ForMember(dest => dest.ReplyToMessage, opt => opt.Ignore());

            CreateMap<MessageMedia, MessageMediaDto>();
            CreateMap<Conversation, ConversationDto>();
            CreateMap<Conversation, ConversationDetailDto>();
        }
    }
}
