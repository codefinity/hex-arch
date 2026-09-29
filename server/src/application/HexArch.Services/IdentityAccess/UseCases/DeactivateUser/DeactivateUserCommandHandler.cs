using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.DeactivateUser
{
    public class DeactivateUserCommandHandler : IDeactivateUserCommandHandler
    {
        private readonly IValidator<DeactivateUserCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public DeactivateUserCommandHandler(
            IValidator<DeactivateUserCommand> validator,
            IUserRepository userRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<DeactivateUserResult> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return DeactivateUserResult.Failed(errors);
            }

            var user = await userRepository.GetUser(command.UserId);
            if (user is null)
            {
                return DeactivateUserResult.Failed("User not found.");
            }

            // Already inactive: succeed without writing, so a redelivered fraud notification
            // (RabbitMQ is at-least-once) is a harmless no-op rather than a failure or a
            // duplicate write.
            if (!user.Active)
            {
                return DeactivateUserResult.Succeeded(user.Id);
            }

            user.Active = false;
            user.DeactivationReason = command.Reason;
            user.DeactivatedOn = clock.UtcNow;
            // Refusing new sign-ins is not enough: tokens already issued must stop working too.
            user.SecurityStamp = Guid.NewGuid();

            await userRepository.UpdateUser(user);

            // Raised only after the user is committed, so any listener projecting a read model
            // always sees durable data.
            await events.Dispatch(new UserDeactivated(user.Id, command.Reason, clock.UtcNow), cancellationToken);

            return DeactivateUserResult.Succeeded(user.Id);
        }
    }
}
