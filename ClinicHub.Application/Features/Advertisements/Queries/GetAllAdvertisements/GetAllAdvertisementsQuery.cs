using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Advertisements.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Queries.GetAllAdvertisements
{
    public class GetAllAdvertisementsQuery : IRequest<PagginatedResult<AdvertisementDto>>
    {
        public AdvertisementStatus? Status { get; set; }
        public Guid? ClinicId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
