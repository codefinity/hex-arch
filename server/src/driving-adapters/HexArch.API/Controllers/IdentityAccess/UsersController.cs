using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        public readonly IRegisterUserCommandHandler registerNewUserCommandHandler;

        public UsersController(IRegisterUserCommandHandler registerNewUserCommandHandler)
        {
            this.registerNewUserCommandHandler = registerNewUserCommandHandler;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var result = await registerNewUserCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return CreatedAtAction(nameof(Register), new { id = result.UserId }, new { id = result.UserId });
        }
    }
}
