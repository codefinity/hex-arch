using HexArch.API.Controllers.IdentityAccess.Requests;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HexArch.API.Controllers.IdentityAccess
{
    /// <summary>
    /// Administrative actions on users. Shares the api/users prefix with UsersController, but lives
    /// apart so the Admin requirement sits on the class and can't be forgotten on a new action.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = RoleNames.Admin)]
    public class UserAdministrationController : ControllerBase
    {
        private readonly ISearchUsersQueryHandler searchUsersQueryHandler;
        private readonly IShowUserQueryHandler showUserQueryHandler;
        private readonly IDeactivateUserCommandHandler deactivateUserCommandHandler;
        private readonly IReactivateUserCommandHandler reactivateUserCommandHandler;
        private readonly IAssignRoleCommandHandler assignRoleCommandHandler;
        private readonly IRevokeRoleCommandHandler revokeRoleCommandHandler;
        private readonly IRevokeSessionsCommandHandler revokeSessionsCommandHandler;

        public UserAdministrationController(
            ISearchUsersQueryHandler searchUsersQueryHandler,
            IShowUserQueryHandler showUserQueryHandler,
            IDeactivateUserCommandHandler deactivateUserCommandHandler,
            IReactivateUserCommandHandler reactivateUserCommandHandler,
            IAssignRoleCommandHandler assignRoleCommandHandler,
            IRevokeRoleCommandHandler revokeRoleCommandHandler,
            IRevokeSessionsCommandHandler revokeSessionsCommandHandler)
        {
            this.searchUsersQueryHandler = searchUsersQueryHandler;
            this.showUserQueryHandler = showUserQueryHandler;
            this.deactivateUserCommandHandler = deactivateUserCommandHandler;
            this.reactivateUserCommandHandler = reactivateUserCommandHandler;
            this.assignRoleCommandHandler = assignRoleCommandHandler;
            this.revokeRoleCommandHandler = revokeRoleCommandHandler;
            this.revokeSessionsCommandHandler = revokeSessionsCommandHandler;
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] bool? active,
            CancellationToken cancellationToken,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = new SearchUsersQuery(search, role, active, page, pageSize);
            var result = await searchUsersQueryHandler.Handle(query, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { users = result.Users, totalCount = result.TotalCount, page = result.Page, pageSize = result.PageSize });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Show(Guid id, CancellationToken cancellationToken)
        {
            var result = await showUserQueryHandler.Handle(new ShowUserQuery(id), cancellationToken);

            if (!result.Success)
            {
                return NotFound(result.Errors);
            }

            return Ok(result.User);
        }

        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id, DeactivateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await deactivateUserCommandHandler.Handle(new DeactivateUserCommand(id, request.Reason), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { userId = result.UserId });
        }

        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> Reactivate(Guid id, ReactivateUserRequest request, CancellationToken cancellationToken)
        {
            var result = await reactivateUserCommandHandler.Handle(new ReactivateUserCommand(id, request.Reason), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { userId = result.UserId });
        }

        [HttpPost("{id:guid}/roles")]
        public async Task<IActionResult> AssignRole(Guid id, AssignRoleRequest request, CancellationToken cancellationToken)
        {
            var result = await assignRoleCommandHandler.Handle(new AssignRoleCommand(id, request.RoleName), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { userId = result.UserId });
        }

        [HttpDelete("{id:guid}/roles/{roleName}")]
        public async Task<IActionResult> RevokeRole(Guid id, string roleName, CancellationToken cancellationToken)
        {
            var result = await revokeRoleCommandHandler.Handle(new RevokeRoleCommand(id, roleName), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { userId = result.UserId });
        }

        [HttpPost("{id:guid}/sessions/revoke")]
        public async Task<IActionResult> RevokeSessions(Guid id, CancellationToken cancellationToken)
        {
            var result = await revokeSessionsCommandHandler.Handle(new RevokeSessionsCommand(id), cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
