using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Invoices.Commands.CancelInvoice;
using ClinicHub.Application.Features.Invoices.Commands.CreateDraftInvoice;
using ClinicHub.Application.Features.Invoices.Commands.IssueInvoice;
using ClinicHub.Application.Features.Invoices.Commands.RecordPayment;
using ClinicHub.Application.Features.Invoices.Commands.UpdateDraftInvoice;
using ClinicHub.Application.Features.Invoices.Queries.GetInvoiceById;
using ClinicHub.Application.Features.Invoices.Queries.GetInvoicesByClinic;
using ClinicHub.Application.Features.Invoices.Queries.GetInvoiceStats;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
[RoleAuthorize]
[RequirePlanPermission(SubscriptionPermission.ManageBilling)]
public class InvoicesController : BaseApiController
{
    public InvoicesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Route(ApiRoutes.Invoices.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetInvoicesByClinicQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.Invoices.Stats)]
    public async Task<IActionResult> GetStats()
    {
        var result = await _mediator.Send(new GetInvoiceStatsQuery());
        return Ok(result);
    }

    [HttpGet]
    [Route(ApiRoutes.Invoices.GetById)]
    public async Task<IActionResult> GetById(Guid invoiceId)
    {
        var result = await _mediator.Send(new GetInvoiceByIdQuery { InvoiceId = invoiceId });
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.Invoices.Create)]
    public async Task<IActionResult> Create([FromBody] CreateDraftInvoiceCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPut]
    [Route(ApiRoutes.Invoices.Update)]
    public async Task<IActionResult> Update(Guid invoiceId, [FromBody] UpdateDraftInvoiceCommand command)
    {
        command.InvoiceId = invoiceId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.Invoices.Issue)]
    public async Task<IActionResult> Issue(Guid invoiceId)
    {
        var result = await _mediator.Send(new IssueInvoiceCommand { InvoiceId = invoiceId });
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.Invoices.Cancel)]
    public async Task<IActionResult> Cancel(Guid invoiceId, [FromBody] CancelInvoiceCommand command)
    {
        command.InvoiceId = invoiceId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.InvoicePayments.Record)]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
