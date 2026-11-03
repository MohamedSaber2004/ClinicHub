using ClinicHub.Application.Features.AdminPayouts.DTOs;
using MediatR;

namespace ClinicHub.Application.Features.AdminPayouts.Commands.CreditClinicLedger;

/// <summary>Adds money the platform owes a clinic outside the appointment flow.</summary>
public class CreditClinicLedgerCommand : IRequest<ClinicLedgerCreditDto>
{
    public Guid ClinicId { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
