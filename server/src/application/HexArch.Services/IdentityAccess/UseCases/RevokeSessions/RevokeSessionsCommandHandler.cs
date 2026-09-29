using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.RevokeSessions
{
    /// <summary>Signs any user out of every device; the administrative counterpart of SignOut.</summary>
    public class RevokeSessionsCommandHandler : IRevokeSessionsCommandHandler
    {
        private readonly IValidator<RevokeSessionsCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public RevokeSessionsCommandHandler(
            IValidator<RevokeSessionsCommand> validator,
            IUserRepository userRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<RevokeSessionsResult> Handle(RevokeSessionsCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return RevokeSessionsResult.Failed(errors);
            }

            var user = await userRepository.GetUser(command.UserId);
            if (user is null)
            {
                return RevokeSessionsResult.Failed("User not found.");
            }

            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserSessionsRevoked(user.Id, clock.UtcNow), cancellationToken);

            return RevokeSessionsResult.Succeeded(user.Id);
        }
    }
}
