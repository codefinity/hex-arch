using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISignInCommandHandler signInCommandHandler;

        public AuthController(ISignInCommandHandler signInCommandHandler)
        {
            this.signInCommandHandler = signInCommandHandler;
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
    }
}
