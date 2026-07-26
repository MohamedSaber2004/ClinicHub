using AutoMapper;
using ClinicHub.Application.Features.ClinicStaff.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.ClinicStaff
{
    public class StaffProfile : Profile
    {
        public StaffProfile()
        {
            CreateMap<ApplicationUser, StaffDto>();
        }
    }
}
