using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs
{
    public class GetClinicAuditLogsQuery : IRequest<PagginatedResult<AuditLogDto>>
    {
        public Guid ClinicId { get; set; }
        public int PageNumber { get; set; } = PagginatedResult<AuditLogDto>.DefaultPageNumber;
        public int PageSize { get; set; } = PagginatedResult<AuditLogDto>.DefaultPageSize;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Action { get; set; }
    }
}
