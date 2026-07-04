using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Doctors.Commands.CreateDoctor;
using ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor;
using ClinicHub.Application.Features.Doctors.Commands.DeleteDoctor;
using ClinicHub.Application.Features.Doctors.Queries.GetDoctorById;
using ClinicHub.Application.Features.Doctors.Queries.GetDoctorsByClinic;
using ClinicHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [RoleAuthorize]
    public class DoctorsController : BaseApiController
    {
        public DoctorsController(IMediator mediator)
            : base(mediator)
        {
        }

        [HttpGet]
        [Route(ApiRoutes.Doctors.GetAllByClinic)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByClinic([FromRoute] Guid clinicId)
        {
            var query = new GetDoctorsByClinicQuery { ClinicId = clinicId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet]
        [Route(ApiRoutes.Doctors.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var query = new GetDoctorByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.Doctors.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateDoctorCommand command)
        {
            var result = await _mediator.Send(command);
            return Created(ApiRoutes.Doctors.GetById, result);
        }

        [HttpPut]
        [Route(ApiRoutes.Doctors.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorCommand command)
        {
            command.DoctorId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.Doctors.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteDoctorCommand { DoctorId = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
