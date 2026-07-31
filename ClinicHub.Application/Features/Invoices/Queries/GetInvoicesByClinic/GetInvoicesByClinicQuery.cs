using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Invoices.DTOs;
using ClinicHub.Domain.Enums;
using MediatR;

namespace ClinicHub.Application.Features.Invoices.Queries.GetInvoicesByClinic;

public class GetInvoicesByClinicQuery : IRequest<PagginatedResult<InvoiceDto>>
{
    public int PageNumber { get; set; } = PagginatedResult<InvoiceDto>.DefaultPageNumber;
    public int PageSize { get; set; } = PagginatedResult<InvoiceDto>.DefaultPageSize;
    public InvoiceStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? PatientId { get; set; }
}
