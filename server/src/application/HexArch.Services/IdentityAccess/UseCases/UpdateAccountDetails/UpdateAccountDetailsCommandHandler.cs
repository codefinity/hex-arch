using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.UpdateAccountDetails
{
    /// <summary>
    /// Changes the details held on the user itself (name, mobile). Kept apart from UpdateProfile,
    /// which only writes the optional Profile record.
    /// </summary>
    public class UpdateAccountDetailsCommandHandler : IUpdateAccountDetailsCommandHandler
    {
        private readonly IValidator<UpdateAccountDetailsCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public UpdateAccountDetailsCommandHandler(
            IValidator<UpdateAccountDetailsCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<UpdateAccountDetailsResult> Handle(UpdateAccountDetailsCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return UpdateAccountDetailsResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return UpdateAccountDetailsResult.Failed("User not found.");
            }

            user.Name = command.Name;
            user.MobileNo = command.MobileNo;

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserAccountDetailsUpdated(user.Id, clock.UtcNow), cancellationToken);

            return UpdateAccountDetailsResult.Succeeded(user.Id);
        }
    }
}
