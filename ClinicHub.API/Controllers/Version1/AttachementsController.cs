using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Attachements.Commands.DownloadFile;
using ClinicHub.Application.Features.Attachements.Commands.UpdateFile;
using ClinicHub.Application.Features.Attachements.Commands.UploadFile;
using ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Attachments;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1;

[ApiVersion("1.0")]
//[RoleAuthorize]
public class AttachementsController : BaseApiController
{
    public AttachementsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [Route(ApiRoutes.Attachments.UploadFile)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadFile([FromForm] UploadFileCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPatch]
    [Route(ApiRoutes.Attachments.UpdateFile)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateFile(string name, [FromForm] UpdateFileCommand command)
    {
        command.OldFileName = name;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.Attachments.UploadMultipleAttachments)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadMultipleAttachments([FromForm] UploadMultipleAttachmentsCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost]
    [Route(ApiRoutes.Attachments.DownloadFile)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadFile([FromForm] DownloadFileCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result);
    }
}
