using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.RejectSellerApplication
{
    public class RejectSellerApplicationCommandHandler : IRejectSellerApplicationCommandHandler
    {
        private readonly IValidator<RejectSellerApplicationCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly ISellerApplicationRepository applicationRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public RejectSellerApplicationCommandHandler(
            IValidator<RejectSellerApplicationCommand> validator,
            ICurrentUserProvider currentUserProvider,
            ISellerApplicationRepository applicationRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.applicationRepository = applicationRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<RejectSellerApplicationResult> Handle(RejectSellerApplicationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return RejectSellerApplicationResult.Failed(errors);
            }

            var application = await applicationRepository.GetApplication(command.ApplicationId);
            if (application is null)
            {
                return RejectSellerApplicationResult.Failed("Seller application not found.");
            }

            if (application.Status != SellerApplicationStatus.Pending)
            {
                return RejectSellerApplicationResult.Failed("This seller application has already been decided.");
            }

            application.Status = SellerApplicationStatus.Rejected;
            application.DecidedOn = clock.UtcNow;
            application.DecidedBy = currentUserProvider.UserId;
            application.DecisionNote = command.Reason;

            await applicationRepository.UpdateApplication(application);

            await events.Dispatch(
                new SellerApplicationRejected(application.Id, application.UserId, command.Reason, clock.UtcNow), cancellationToken);

            return RejectSellerApplicationResult.Succeeded(application.Id);
        }
    }
}
