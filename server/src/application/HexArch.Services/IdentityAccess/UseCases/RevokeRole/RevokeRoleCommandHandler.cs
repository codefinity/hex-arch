using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.RevokeRole
{
    public class RevokeRoleCommandHandler : IRevokeRoleCommandHandler
    {
        private readonly IValidator<RevokeRoleCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public RevokeRoleCommandHandler(
            IValidator<RevokeRoleCommand> validator,
            IUserRepository userRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<RevokeRoleResult> Handle(RevokeRoleCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return RevokeRoleResult.Failed(errors);
            }

            var user = await userRepository.GetUser(command.UserId);
            if (user is null)
            {
                return RevokeRoleResult.Failed("User not found.");
            }

            var role = user.Roles.FirstOrDefault(existing => existing.Name == command.RoleName);
            if (role is null)
            {
                return RevokeRoleResult.Succeeded(user.Id);
            }

            if (user.Roles.Count == 1)
            {
                return RevokeRoleResult.Failed("A user must keep at least one role.");
            }

            if (role.Name == RoleNames.Admin && user.Active
                && await userRepository.CountActiveUsersInRole(RoleNames.Admin) <= 1)
            {
                return RevokeRoleResult.Failed("The last active administrator cannot lose the Admin role.");
            }

            user.Roles.Remove(role);
            // Unlike a grant, a revocation must reach tokens already issued: they still claim the role.
            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            await events.Dispatch(
                new UserRolesChanged(user.Id, user.Roles.Select(r => r.Name).ToArray(), clock.UtcNow), cancellationToken);

            return RevokeRoleResult.Succeeded(user.Id);
        }
    }
}
