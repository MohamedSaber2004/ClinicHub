using AutoMapper;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;

namespace ClinicHub.Application.Features.AdminPayments;

public class AdminPaymentsProfile : Profile
{
    public AdminPaymentsProfile()
    {
        CreateMap<AdPackage, AdPackageDto>();
    }
}
