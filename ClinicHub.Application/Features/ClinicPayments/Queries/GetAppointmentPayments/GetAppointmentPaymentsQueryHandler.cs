using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.AdminPayments;
using ClinicHub.Application.Features.ClinicPayments.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentEntity = ClinicHub.Domain.Entities.Payment;

namespace ClinicHub.Application.Features.ClinicPayments.Queries.GetAppointmentPayments;

public class GetAppointmentPaymentsQueryHandler
    : IRequestHandler<GetAppointmentPaymentsQuery, PagginatedResult<AppointmentPaymentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentPaymentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagginatedResult<AppointmentPaymentDto>> Handle(GetAppointmentPaymentsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.CurrentClinicId is null)
            return new PagginatedResult<AppointmentPaymentDto>(new List<AppointmentPaymentDto>(), 0, request.PageNumber, request.PageSize);

        var query = _unitOfWork.PaymentRepository
            .GetAllAsync(null)
            .Include(p => p.Appointment)
                .ThenInclude(a => a!.Doctor)
                    .ThenInclude(d => d.User)
            .Where(p => p.Type == PaymentType.Appointment
                        && p.ClinicId == _currentUserService.CurrentClinicId.Value
                        && p.Appointment != null);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == (PaymentStatus)request.Status.Value
                                     || (request.Status == (int)PaymentStatus.Pending && p.Status == PaymentStatus.Processing));

        query = query.OrderByDescending(p => p.Appointment!.AppointmentDate)
                     .ThenByDescending(p => p.Appointment!.StartTime);

        var payments = await query.ToListAsync(cancellationToken);

        if (request.Method.HasValue)
            payments = payments.Where(p => PaymentMethodMapper.ToEnum(p.PaymentMethod) == (PaymentMethod)request.Method.Value).ToList();

        var pageNumber = request.PageNumber < 1 ? PagginatedResult<AppointmentPaymentDto>.DefaultPageNumber : request.PageNumber;
        var pageSize = request.PageSize < 1 ? PagginatedResult<AppointmentPaymentDto>.DefaultPageSize
                     : request.PageSize > PagginatedResult<AppointmentPaymentDto>.MaxPageSize ? PagginatedResult<AppointmentPaymentDto>.MaxPageSize
                     : request.PageSize;

        var totalCount = payments.Count;
        var items = payments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDto)
            .ToList();

        return new PagginatedResult<AppointmentPaymentDto>(items, totalCount, pageNumber, pageSize);
    }

    private static AppointmentPaymentDto ToDto(PaymentEntity p) => new()
    {
        Id = p.Id,
        PatientName = p.Appointment!.PatientFullName,
        DoctorName = p.Appointment.Doctor?.User?.FullName ?? string.Empty,
        AppointmentDate = p.Appointment.AppointmentDate,
        StartTime = p.Appointment.StartTime.ToString(@"hh\:mm"),
        Amount = p.Amount,
        Currency = p.Currency,
        Method = PaymentMethodMapper.ToEnum(p.PaymentMethod),
        Status = PaymentMethodMapper.ToUiStatus(p.Status)
    };
}
