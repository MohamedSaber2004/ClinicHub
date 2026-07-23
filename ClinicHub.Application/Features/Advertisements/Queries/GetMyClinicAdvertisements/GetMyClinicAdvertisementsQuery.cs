using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Advertisements.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Advertisements.Queries.GetMyClinicAdvertisements
{
    public class GetMyClinicAdvertisementsQuery : IRequest<PagginatedResult<AdvertisementDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
