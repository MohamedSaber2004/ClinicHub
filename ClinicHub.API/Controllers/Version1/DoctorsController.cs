using Asp.Versioning;
using ClinicHub.API.Filters;
using ClinicHub.API.Routes;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.ClinicStaff.Commands.ChangePassword;
using ClinicHub.Application.Features.Doctors.Commands.CreateDoctor;
using ClinicHub.Application.Features.Doctors.Commands.UpdateDoctor;
using ClinicHub.Application.Features.Doctors.Commands.DeleteDoctor;
using ClinicHub.Application.Features.Doctors.Queries.GetDoctorById;
using ClinicHub.Application.Features.Doctors.Queries.GetDoctorDetailsForMobile;
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
        private readonly ICurrentUserService _currentUserService;

        public DoctorsController(IMediator mediator, ICurrentUserService currentUserService)
            : base(mediator)
        {
            _currentUserService = currentUserService;
        }

        [HttpGet]
        [Route(ApiRoutes.Doctors.GetAllByClinic)]
        [RequirePlanPermission(SubscriptionPermission.ManageDoctors)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByClinic([FromRoute] Guid clinicId, [FromQuery] GetDoctorsByClinicQuery query)
        {
            query.ClinicId = clinicId;
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

        [HttpGet]
        [Route(ApiRoutes.Doctors.GetDetailsForMobile)]
        [RoleAuthorize(nameof(UserType.User))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailsForMobile([FromRoute] Guid doctorId, CancellationToken ct)
        {
            var query = new GetDoctorDetailsForMobileQuery { DoctorId = doctorId };
            var result = await _mediator.Send(query, ct);
            return Ok(result);
        }

        [HttpPost]
        [Route(ApiRoutes.Doctors.Create)]
        [RoleAuthorize(nameof(UserType.SuperAdmin))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateDoctorCommand command)
        {
            var result = await _mediator.Send(command);
            return Created(ApiRoutes.Doctors.GetById, result);
        }

        [HttpPost]
        [Route(ApiRoutes.ClinicManagement.BaseRoute + "/doctors")]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [RequirePlanPermission(SubscriptionPermission.ManageDoctors)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateForMyClinic([FromBody] CreateDoctorCommand command)
        {
            var clinicId = _currentUserService.CurrentClinicId
                ?? throw new InvalidOperationException("ClinicOwner must have a clinic assigned.");
            command.ClinicId = clinicId;
            var result = await _mediator.Send(command);
            return Created(ApiRoutes.Doctors.GetById, result);
        }

        [HttpPut]
        [Route(ApiRoutes.Doctors.Update)]
        [RoleAuthorize(nameof(UserType.SuperAdmin), nameof(UserType.ClinicOwner))]
        [RequirePlanPermission(SubscriptionPermission.ManageDoctors)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDoctorCommand command)
        {
            command.DoctorId = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        [Route(ApiRoutes.Doctors.ChangePassword)]
        [RoleAuthorize(nameof(UserType.ClinicOwner))]
        [RequirePlanPermission(SubscriptionPermission.ManageDoctors)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangeClinicUserPasswordCommand command, CancellationToken ct)
        {
            command = command with { UserId = id };
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete]
        [Route(ApiRoutes.Doctors.Delete)]
        [RoleAuthorize(nameof(UserType.SuperAdmin), nameof(UserType.ClinicOwner))]
        [RequirePlanPermission(SubscriptionPermission.ManageDoctors)]
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
