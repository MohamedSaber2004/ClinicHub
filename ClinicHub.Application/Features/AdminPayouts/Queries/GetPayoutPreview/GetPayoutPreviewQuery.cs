using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutPreview;

public class GetPayoutPreviewQuery : IRequest<PayoutPreviewDto>
{
    public decimal? MinimumAmount { get; set; }
}
