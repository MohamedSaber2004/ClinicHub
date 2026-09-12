using ClinicHub.Application.Common;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.AdminPayments.Queries.GetAdminPayments;

public class GetAdminPaymentsQueryHandler : IRequestHandler<GetAdminPaymentsQuery, PagginatedResult<AdminPaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAdminPaymentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagginatedResult<AdminPaymentDto>> Handle(GetAdminPaymentsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.PaymentRepository
            .GetAllAsync(null)
            .Include(p => p.Clinic)
            .Include(p => p.Appointment)
                .ThenInclude(a => a!.BookedByUser)
            .AsQueryable();

        if (request.Type.HasValue)
            query = query.Where(p => p.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.FromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= request.FromDate.Value.Date);

        if (request.ToDate.HasValue)
            query = query.Where(p => p.CreatedAt < request.ToDate.Value.Date.AddDays(1));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p =>
                (p.Code != null && p.Code.Contains(term)) ||
                (p.RefNumber != null && p.RefNumber.Contains(term)) ||
                (p.Clinic != null && p.Clinic.Name.Contains(term)) ||
                (p.Appointment != null && p.Appointment.PatientFullName.Contains(term)));
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var payments = await query.ToListAsync(cancellationToken);

        var percent = await GetPlatformFeePercentAsync(cancellationToken);

        if (request.Method.HasValue)
            payments = payments.Where(p => PaymentMethodMapper.ToEnum(p.PaymentMethod) == request.Method.Value).ToList();

        var pageNumber = request.PageNumber < 1 ? PagginatedResult<AdminPaymentDto>.DefaultPageNumber : request.PageNumber;
        var pageSize = request.PageSize < 1 ? PagginatedResult<AdminPaymentDto>.DefaultPageSize
                     : request.PageSize > PagginatedResult<AdminPaymentDto>.MaxPageSize ? PagginatedResult<AdminPaymentDto>.MaxPageSize
                     : request.PageSize;

        var totalCount = payments.Count;
        var items = payments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p =>
            {
                var split = p.Type == PaymentType.Appointment
                    ? AppointmentRevenueSplitter.Split(p.Amount, percent)
                    : new AppointmentRevenueSplit(p.Amount, 0m, p.Amount);
                return new AdminPaymentDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Type = p.Type,
                    Payer = ResolvePayer(p),
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Method = PaymentMethodMapper.ToEnum(p.PaymentMethod),
                    Status = PaymentMethodMapper.ToUiStatus(p.Status),
                    Date = p.CreatedAt,
                    RefNumber = p.RefNumber,
                    PlatformFee = split.PlatformFee,
                    ClinicNetAmount = split.ClinicNet
                };
            })
            .ToList();

        return new PagginatedResult<AdminPaymentDto>(items, totalCount, pageNumber, pageSize);
    }

    private async Task<decimal> GetPlatformFeePercentAsync(CancellationToken cancellationToken)
    {
        var setting = await _unitOfWork.GetRepository<PlatformSetting, Guid>()
            .GetAllAsync(s => !s.IsDeleted)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return setting?.AppointmentFeePercent ?? 0m;
    }

    private static string ResolvePayer(ClinicHub.Domain.Entities.Payment payment) =>
        payment.Type == PaymentType.Appointment && payment.Appointment != null
            ? payment.Appointment.PatientFullName
            : payment.Clinic?.Name ?? payment.UserId.ToString();
}
