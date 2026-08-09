using Asp.Versioning;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.API.Routes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class DeepLinkVerifyController : BaseApiController
    {
        private readonly IDeepLinkService _deepLinkService;

        public DeepLinkVerifyController(IMediator mediator, IDeepLinkService deepLinkService) : base(mediator)
        {
            _deepLinkService = deepLinkService;
        }

        /// <summary>
        /// Verify a deep link token issued by the API.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.DeepLinks.Verify)]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Verify([FromBody] VerifyDeepLinkRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Data) || string.IsNullOrWhiteSpace(request.Token))
                return BadRequest("Data and token are required.");

            var isValid = _deepLinkService.VerifyToken(request.Data, request.Token);
            return Ok(new { valid = isValid });
        }
    }

    public class VerifyDeepLinkRequest
    {
        public string Data { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
