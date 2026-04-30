using Asp.Versioning;
using ClinicHub.Application.Features.Auth.Commands.ForgetPassword;
using ClinicHub.Application.Features.Auth.Commands.Login;
using ClinicHub.Application.Features.Auth.Commands.RefreshToken;
using ClinicHub.Application.Features.Auth.Commands.ResetPassword;
using ClinicHub.Application.Features.Auth.Commands.Signup;
using ClinicHub.Application.Features.Auth.Commands.VerifyResetToken;
using ClinicHub.Application.Features.Auth.Commands.VerifyUser;
using ClinicHub.Application.Features.Auth.Queries.GetUserProfile;
using ClinicHub.Application.Features.Auth.Commands.UpdateProfile;
using ClinicHub.Application.Features.Auth.Commands.UpdateLanguage;
using ClinicHub.Application.Features.Auth.Commands.LoginWithFacebook;
using ClinicHub.Application.Features.Auth.Commands.LoginWithGoogle;
using ClinicHub.Application.Features.Auth.Commands.CompleteFacebookRegistration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Auth.Commands.ValidateGoogleAccessToken;
using ClinicHub.Application.Features.Auth.Commands.Logout;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    public class AuthController : BaseApiController
    {
        public AuthController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="command">Registration details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication tokens and user info.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.Signup)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Signup(SignupCommand command, CancellationToken ct)
        {
            var result  = await _mediator.Send(command, ct);
            return Created(null!,result);
        }

        /// <summary>
        /// Verifies a user using a code sent via email.
        /// </summary>
        /// <param name="command">Email and verification code.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication tokens and user info.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.Verify)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Verify(VerifyUserCommand command, CancellationToken ct)
        {
            var result  = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Logins a user and issues tokens.
        /// </summary>
        /// <param name="command">Login credentials.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication tokens and user info.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.Login)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
        {
            var result  = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Logins a user using Facebook access token.
        /// </summary>
        /// <param name="command">Facebook access token.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication tokens and user info.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.LoginWithFacebook)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginWithFacebook(LoginWithFacebookCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Logins a user using Google ID token.
        /// </summary>
        /// <param name="command">Google ID token.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Authentication tokens and user info.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.LoginWithGoogle)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginWithGoogle(LoginWithGoogleCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Validate on googel token
        /// </summary>
        /// <param name="command"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.ValidateGoogleToken)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateGoogleToken(ValidateGoogleAccessTokenCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Completes Facebook registration by providing a manual email.
        /// </summary>
        [HttpPost]
        [Route(ApiRoutes.Auth.CompleteFacebookRegistration)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompleteFacebookRegistration(CompleteFacebookRegistrationCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Refreshes the authentication tokens using a refresh token.
        /// </summary>
        /// <param name="command">The refresh token.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>New authentication tokens.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.RefreshToken)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command, CancellationToken ct)
        {
            var result  = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Initiates the password reset process by sending an email.
        /// </summary>
        /// <param name="command">The user's email address.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.ForgetPassword)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Verifies a password reset token.
        /// </summary>
        /// <param name="command">Email and token.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>True if the token is valid, otherwise false.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.VerifyResetToken)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyResetToken(VerifyResetTokenCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Resets the user's password.
        /// </summary>
        /// <param name="command">New password details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.ResetPassword)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Gets the profile of the current authenticated user.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>The user's profile info.</returns>
        [HttpGet]
        [Authorize]
        [Route(ApiRoutes.Auth.Profile)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUserProfileQuery(), ct);
            return Ok(result);
        }

        /// <summary>
        /// Updates the profile of the current authenticated user.
        /// </summary>
        /// <param name="command">Profile details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        [HttpPut]
        [Authorize]
        [Route(ApiRoutes.Auth.UpdateProfile)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Updates the language preference of the current authenticated user.
        /// </summary>
        /// <param name="command">Language preference details.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        [HttpPut]
        [Authorize]
        [Route(ApiRoutes.Auth.UpdateLanguage)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateLanguage(UpdateLanguageCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        /// <summary>
        /// Logs out the user by revoking the refresh token.
        /// </summary>
        /// <param name="command">The logout details including refresh token.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>A success message.</returns>
        [HttpPost]
        [Route(ApiRoutes.Auth.Logout)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(LogoutCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }
}
