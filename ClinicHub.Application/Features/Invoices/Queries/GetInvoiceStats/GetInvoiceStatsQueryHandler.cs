using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoiceStats;

public class GetInvoiceStatsQueryHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService
) : IRequestHandler<GetInvoiceStatsQuery, InvoiceStatsDto>
{
    public async Task<InvoiceStatsDto> Handle(GetInvoiceStatsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = currentUserService.CurrentClinicId
            ?? throw new InvalidOperationException("User must be associated with a clinic.");

        var (todayRevenue, paidCount, pendingCount, draftCount, cancelledCount) =
            await unitOfWork.InvoiceRepository.GetInvoiceStatsAsync(clinicId);

        return new InvoiceStatsDto
        {
            TodayRevenue = todayRevenue,
            PaidCount = paidCount,
            PendingCount = pendingCount,
            DraftCount = draftCount,
            CancelledCount = cancelledCount,
            InsuranceRatio = 0
        };
    }
}
