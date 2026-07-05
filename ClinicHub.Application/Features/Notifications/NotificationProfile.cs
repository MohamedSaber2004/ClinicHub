using AutoMapper;
using ClinicHub.Application.Features.Notifications.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.Notifications
{
    public class NotificationProfile : Profile
    {
        public NotificationProfile()
        {
            CreateMap<Notification, NotificationDto>();
        }
    }
}
