using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    /// <summary>Everything the signed-in user can do to their own account, beyond their profile.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IUpdateAccountDetailsCommandHandler updateAccountDetailsCommandHandler;
        private readonly IChangePasswordCommandHandler changePasswordCommandHandler;
        private readonly IChangeEmailCommandHandler changeEmailCommandHandler;
        private readonly IRequestEmailVerificationCommandHandler requestEmailVerificationCommandHandler;
        private readonly IVerifyEmailCommandHandler verifyEmailCommandHandler;
        private readonly ICloseAccountCommandHandler closeAccountCommandHandler;
        private readonly IApplyForSellerAccountCommandHandler applyForSellerAccountCommandHandler;

        public AccountController(
            IUpdateAccountDetailsCommandHandler updateAccountDetailsCommandHandler,
            IChangePasswordCommandHandler changePasswordCommandHandler,
            IChangeEmailCommandHandler changeEmailCommandHandler,
            IRequestEmailVerificationCommandHandler requestEmailVerificationCommandHandler,
            IVerifyEmailCommandHandler verifyEmailCommandHandler,
            ICloseAccountCommandHandler closeAccountCommandHandler,
            IApplyForSellerAccountCommandHandler applyForSellerAccountCommandHandler)
        {
            this.updateAccountDetailsCommandHandler = updateAccountDetailsCommandHandler;
            this.changePasswordCommandHandler = changePasswordCommandHandler;
            this.changeEmailCommandHandler = changeEmailCommandHandler;
            this.requestEmailVerificationCommandHandler = requestEmailVerificationCommandHandler;
            this.verifyEmailCommandHandler = verifyEmailCommandHandler;
            this.closeAccountCommandHandler = closeAccountCommandHandler;
            this.applyForSellerAccountCommandHandler = applyForSellerAccountCommandHandler;
        }

        [HttpPut("details")]
        public async Task<IActionResult> UpdateDetails(UpdateAccountDetailsCommand command, CancellationToken cancellationToken)
        {
            var result = await updateAccountDetailsCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { userId = result.UserId });
        }

        /// <summary>Returns a fresh token: the change signs out every session, the caller's included.</summary>
        [HttpPost("password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await changePasswordCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { token = result.Token, expiresOnUtc = result.ExpiresOnUtc });
        }

        /// <summary>Starts an email change; the new address takes effect once verified.</summary>
        [HttpPost("email")]
        public async Task<IActionResult> ChangeEmail(ChangeEmailCommand command, CancellationToken cancellationToken)
        {
            var result = await changeEmailCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Accepted();
        }

        [HttpPost("email/verification")]
        public async Task<IActionResult> RequestEmailVerification(CancellationToken cancellationToken)
        {
            var result = await requestEmailVerificationCommandHandler.Handle(new RequestEmailVerificationCommand(), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Accepted();
        }

        [HttpPost("email/verify")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailCommand command, CancellationToken cancellationToken)
        {
            var result = await verifyEmailCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpPost("close")]
        public async Task<IActionResult> Close(CloseAccountCommand command, CancellationToken cancellationToken)
        {
            var result = await closeAccountCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpPost("seller-application")]
        public async Task<IActionResult> ApplyForSellerAccount(ApplyForSellerAccountCommand command, CancellationToken cancellationToken)
        {
            var result = await applyForSellerAccountCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { applicationId = result.ApplicationId });
        }
    }
}
