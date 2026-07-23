using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Advertisements.Commands.CreateAdvertisement;
using ClinicHub.Application.Features.Advertisements.Commands.DeleteAdvertisement;
using ClinicHub.Application.Features.Advertisements.Commands.UpdateAdvertisement;
using ClinicHub.Application.Features.Advertisements.Queries.GetMyClinicAdvertisements;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize(nameof(UserType.ClinicOwner))]
    public class AdvertisementsController : BaseApiController
    {
        public AdvertisementsController(IMediator mediator) : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.Advertisements.MyAdvertisements)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyAdvertisements([FromQuery] GetMyClinicAdvertisementsQuery query, CancellationToken ct)
        {
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.Advertisements.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateAdvertisementCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Created(ApiRoutes.Advertisements.Create, result);
        }

        [HttpPut]
        [Route(ApiRoutes.Advertisements.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdvertisementCommand command, CancellationToken ct)
        {
            command.Id = id;
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.Advertisements.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteAdvertisementCommand { Id = id }, ct);
            return Ok(result);
        }
    }
}
