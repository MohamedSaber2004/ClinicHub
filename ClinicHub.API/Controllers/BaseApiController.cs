using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Localization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ClinicHub.API.Controllers
{
    [ApiController]
    public class BaseApiController: Controller
    {
        public readonly IMediator _mediator;

        protected BaseApiController(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected IActionResult Ok(string message) => base.Ok(ApiResponse<string>.Ok(null, message ?? LocalizationKeys.ActionResults.Ok));
        protected IActionResult Ok<TData>(TData? data, string message = null!) => base.Ok(ApiResponse<TData>.Ok(data, message ?? LocalizationKeys.ActionResults.Ok));
        protected IActionResult Ok2<TData>(TData? data, string message = null!) => base.Ok(Ok(data, message ?? LocalizationKeys.ActionResults.Ok));
        protected IActionResult Deleted<TData>(string uri, TData data, string message = null!) => base.Accepted(uri, ApiResponse<TData>.Ok(data, message ?? LocalizationKeys.ActionResults.Deleted));
        protected IActionResult Accepted<TData>(string uri, TData data, string message = null!) => base.Accepted(uri, ApiResponse<TData>.Ok(data, message ?? LocalizationKeys.ActionResults.Accepted));
        protected IActionResult Created<TData>(string uri, TData data, string message = null!) => base.Created(uri, ApiResponse<TData>.Ok(data, message ?? LocalizationKeys.ActionResults.Created));
        protected IActionResult Deleted<TData>(TData data, string message = null!) => base.Accepted(ApiResponse<TData>.Ok(data, message ?? LocalizationKeys.ActionResults.Deleted));
        protected IActionResult Accepted<TData>(TData data, string message = null!) => base.Accepted(ApiResponse<TData>.Ok(data, message ?? LocalizationKeys.ActionResults.Accepted));
    }
}
