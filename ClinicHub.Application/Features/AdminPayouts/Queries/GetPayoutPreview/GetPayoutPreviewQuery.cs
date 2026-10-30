using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Queries.GetPayoutPreview;

public class GetPayoutPreviewQuery : IRequest<PayoutPreviewDto>
{
    public decimal? MinimumAmount { get; set; }

    /// <summary>Narrows the preview to one clinic, which is then reported even at zero balance.</summary>
    public Guid? ClinicId { get; set; }
}
