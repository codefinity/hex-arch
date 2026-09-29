using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.ReactivateUser
{
    public class ReactivateUserCommandHandler : IReactivateUserCommandHandler
    {
        private readonly IValidator<ReactivateUserCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public ReactivateUserCommandHandler(
            IValidator<ReactivateUserCommand> validator,
            IUserRepository userRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<ReactivateUserResult> Handle(ReactivateUserCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ReactivateUserResult.Failed(errors);
            }

            var user = await userRepository.GetUser(command.UserId);
            if (user is null)
            {
                return ReactivateUserResult.Failed("User not found.");
            }

            if (user.ClosedOn is not null)
            {
                return ReactivateUserResult.Failed("This account has been closed.");
            }

            // Already active: a no-op, for the same at-least-once reason as DeactivateUser.
            if (user.Active)
            {
                return ReactivateUserResult.Succeeded(user.Id);
            }

            // A fraud clearance must not undo a deactivation made for some other reason (e.g. an
            // administrator suspending the account for abuse), so the caller can make this conditional.
            if (command.OnlyIfDeactivatedFor is not null && user.DeactivationReason != command.OnlyIfDeactivatedFor)
            {
                return ReactivateUserResult.Succeeded(user.Id);
            }

            user.Active = true;
            user.DeactivationReason = null;
            user.DeactivatedOn = null;

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserReactivated(user.Id, command.Reason, clock.UtcNow), cancellationToken);

            return ReactivateUserResult.Succeeded(user.Id);
        }
    }
}
