using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUpdateProfileCommandHandler updateProfileCommandHandler;
        private readonly IShowUserProfileQueryHandler showUserProfileQueryHandler;

        public ProfileController(
            IUpdateProfileCommandHandler updateProfileCommandHandler,
            IShowUserProfileQueryHandler showUserProfileQueryHandler)
        {
            this.updateProfileCommandHandler = updateProfileCommandHandler;
            this.showUserProfileQueryHandler = showUserProfileQueryHandler;
        }

        [HttpPost("profile")]
        public async Task<IActionResult> ShowProfile(CancellationToken cancellationToken)
        {
            var result = await showUserProfileQueryHandler.Handle(new ShowUserProfileQuery(), cancellationToken);

            if (!result.Success)
            {
                return NotFound(result.Errors);
            }

            return Ok(result.Profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var result = await updateProfileCommandHandler.Handle(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { userId = result.UserId });
        }
    }
}
