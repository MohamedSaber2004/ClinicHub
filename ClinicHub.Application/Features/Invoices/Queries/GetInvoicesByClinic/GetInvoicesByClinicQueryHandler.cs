using AutoMapper;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoicesByClinic;

public class GetInvoicesByClinicQueryHandler(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ICurrentUserService currentUserService
) : IRequestHandler<GetInvoicesByClinicQuery, PagginatedResult<InvoiceDto>>
{
    public async Task<PagginatedResult<InvoiceDto>> Handle(GetInvoicesByClinicQuery request, CancellationToken cancellationToken)
    {
        var clinicId = currentUserService.CurrentClinicId
            ?? throw new InvalidOperationException("User must be associated with a clinic.");

        var (items, totalCount) = await unitOfWork.InvoiceRepository.GetByClinicIdPaginatedAsync(
            clinicId,
            request.PageNumber,
            request.PageSize,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.PatientId);

        var dtos = mapper.Map<List<InvoiceDto>>(items);
        return new PagginatedResult<InvoiceDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
