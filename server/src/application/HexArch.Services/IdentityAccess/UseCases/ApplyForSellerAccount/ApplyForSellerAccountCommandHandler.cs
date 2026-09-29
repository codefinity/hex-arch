using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.ApplyForSellerAccount
{
    public class ApplyForSellerAccountCommandHandler : IApplyForSellerAccountCommandHandler
    {
        private readonly IValidator<ApplyForSellerAccountCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly ISellerApplicationRepository applicationRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public ApplyForSellerAccountCommandHandler(
            IValidator<ApplyForSellerAccountCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            ISellerApplicationRepository applicationRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.applicationRepository = applicationRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<ApplyForSellerAccountResult> Handle(ApplyForSellerAccountCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ApplyForSellerAccountResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return ApplyForSellerAccountResult.Failed("User not found.");
            }

            if (user.Roles.Any(role => role.Name == RoleNames.Seller))
            {
                return ApplyForSellerAccountResult.Failed("You are already a seller.");
            }

            if (await applicationRepository.GetPendingApplicationForUser(user.Id) is not null)
            {
                return ApplyForSellerAccountResult.Failed("You already have a pending seller application.");
            }

            var application = new SellerApplication
            {
                UserId = user.Id,
                BusinessName = command.BusinessName,
                Status = SellerApplicationStatus.Pending,
                SubmittedOn = clock.UtcNow
            };

            await applicationRepository.AddApplication(application);

            await events.Dispatch(
                new SellerApplicationSubmitted(application.Id, user.Id, application.BusinessName, clock.UtcNow), cancellationToken);

            return ApplyForSellerAccountResult.Succeeded(application.Id);
        }
    }
}
