using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.DeactivateUser
{
    public class DeactivateUserCommandHandler : IDeactivateUserCommandHandler
    {
        private readonly IValidator<DeactivateUserCommand> validator;
        private readonly IUserRepository userRepository;

        public DeactivateUserCommandHandler(
            IValidator<DeactivateUserCommand> validator,
            IUserRepository userRepository)
        {
            this.validator = validator;
            this.userRepository = userRepository;
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

            await userRepository.UpdateUser(user);

            return DeactivateUserResult.Succeeded(user.Id);
        }
    }
}
