using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISignInCommandHandler signInCommandHandler;
        private readonly ISignOutCommandHandler signOutCommandHandler;
        private readonly IRequestPasswordResetCommandHandler requestPasswordResetCommandHandler;
        private readonly IResetPasswordCommandHandler resetPasswordCommandHandler;

        public AuthController(
            ISignInCommandHandler signInCommandHandler,
            ISignOutCommandHandler signOutCommandHandler,
            IRequestPasswordResetCommandHandler requestPasswordResetCommandHandler,
            IResetPasswordCommandHandler resetPasswordCommandHandler)
        {
            this.signInCommandHandler = signInCommandHandler;
            this.signOutCommandHandler = signOutCommandHandler;
            this.requestPasswordResetCommandHandler = requestPasswordResetCommandHandler;
            this.resetPasswordCommandHandler = resetPasswordCommandHandler;
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn(SignInCommand command, CancellationToken cancellationToken)
        {
            var result = await signInCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return Unauthorized(result.Errors);
            }

            return Ok(new { token = result.Token, expiresOnUtc = result.ExpiresOnUtc });
        }

        /// <summary>Signs the caller out of every device.</summary>
        [Authorize]
        [HttpPost("sign-out")]
        // Not named SignOut, which would sit beside ControllerBase's own SignOut overloads.
        public async Task<IActionResult> SignOutEverywhere(CancellationToken cancellationToken)
        {
            var result = await signOutCommandHandler.Handle(new SignOutCommand(), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        /// <summary>Always 202 for a well-formed email, whether or not it belongs to an account.</summary>
        [HttpPost("password/forgot")]
        public async Task<IActionResult> RequestPasswordReset(RequestPasswordResetCommand command, CancellationToken cancellationToken)
        {
            var result = await requestPasswordResetCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Accepted();
        }

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await resetPasswordCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
