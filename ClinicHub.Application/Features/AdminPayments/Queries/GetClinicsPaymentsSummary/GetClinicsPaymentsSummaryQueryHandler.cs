using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetClinicsPaymentsSummary;

public class GetClinicsPaymentsSummaryQueryHandler
    : IRequestHandler<GetClinicsPaymentsSummaryQuery, PagginatedResult<ClinicPaymentsSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetClinicsPaymentsSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagginatedResult<ClinicPaymentsSummaryDto>> Handle(
        GetClinicsPaymentsSummaryQuery request, CancellationToken cancellationToken)
    {
        var clinicsQuery = _unitOfWork.GetRepository<Clinic, Guid>()
            .GetAllAsync(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            clinicsQuery = clinicsQuery.Where(c => c.Name.Contains(term));
        }

        var clinics = await clinicsQuery
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(cancellationToken);

        var paymentsQuery = _unitOfWork.PaymentRepository
            .GetAllAsync(p => p.Status == PaymentStatus.Paid);

        if (request.FromDate.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date >= request.FromDate.Value.Date);

        if (request.ToDate.HasValue)
            paymentsQuery = paymentsQuery.Where(p => p.PaidAt.HasValue && p.PaidAt.Value.Date <= request.ToDate.Value.Date);

        // One round-trip: per-clinic rows grouped in memory, split per appointment row then summed.
        var rows = await paymentsQuery
            .Select(p => new { p.ClinicId, p.Amount, p.Type })
            .ToListAsync(cancellationToken);

        var percent = await GetPlatformFeePercentAsync(cancellationToken);

        var byClinic = rows
            .GroupBy(r => r.ClinicId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var (fees, net) = AppointmentRevenueSplitter.SumSplits(
                        g.Where(x => x.Type == PaymentType.Appointment).Select(x => x.Amount), percent);
                    return (Count: g.Count(), Gross: g.Sum(x => x.Amount), Fees: fees, Net: net);
                });

        var pageNumber = request.PageNumber < 1 ? PagginatedResult<ClinicPaymentsSummaryDto>.DefaultPageNumber : request.PageNumber;
        var pageSize = request.PageSize < 1 ? PagginatedResult<ClinicPaymentsSummaryDto>.DefaultPageSize
                     : request.PageSize > PagginatedResult<ClinicPaymentsSummaryDto>.MaxPageSize ? PagginatedResult<ClinicPaymentsSummaryDto>.MaxPageSize
                     : request.PageSize;

        var totalCount = clinics.Count;
        var items = clinics
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c =>
            {
                byClinic.TryGetValue(c.Id, out var v);
                return new ClinicPaymentsSummaryDto
                {
                    ClinicId = c.Id,
                    ClinicName = c.Name,
                    PaymentsCount = v.Count,
                    TotalRevenue = v.Gross,
                    PlatformFees = v.Fees,
                    NetRevenue = v.Net
                };
            })
            .ToList();

        return new PagginatedResult<ClinicPaymentsSummaryDto>(items, totalCount, pageNumber, pageSize);
    }

    private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return setting?.AppointmentFeePercent ?? 0m;
    }
}
