using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Admin.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetClinicAuditLogs
{
    public class GetClinicAuditLogsQuery : IRequest<PagginatedResult<AuditLogDto>>
    {
        public Guid ClinicId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
