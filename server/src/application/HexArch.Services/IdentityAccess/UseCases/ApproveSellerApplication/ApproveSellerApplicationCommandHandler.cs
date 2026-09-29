using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.ApproveSellerApplication
{
    public class ApproveSellerApplicationCommandHandler : IApproveSellerApplicationCommandHandler
    {
        private readonly IValidator<ApproveSellerApplicationCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly ISellerApplicationRepository applicationRepository;
        private readonly IUserRepository userRepository;
        private readonly IRoleRepository roleRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public ApproveSellerApplicationCommandHandler(
            IValidator<ApproveSellerApplicationCommand> validator,
            ICurrentUserProvider currentUserProvider,
            ISellerApplicationRepository applicationRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.applicationRepository = applicationRepository;
            this.userRepository = userRepository;
            this.roleRepository = roleRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<ApproveSellerApplicationResult> Handle(ApproveSellerApplicationCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ApproveSellerApplicationResult.Failed(errors);
            }

            var application = await applicationRepository.GetApplication(command.ApplicationId);
            if (application is null)
            {
                return ApproveSellerApplicationResult.Failed("Seller application not found.");
            }

            if (application.Status != SellerApplicationStatus.Pending)
            {
                return ApproveSellerApplicationResult.Failed("This seller application has already been decided.");
            }

            var user = await userRepository.GetUser(application.UserId);
            if (user is null)
            {
                return ApproveSellerApplicationResult.Failed("User not found.");
            }

            if (!user.Active)
            {
                return ApproveSellerApplicationResult.Failed("The applicant's account is not active.");
            }

            var sellerRole = await roleRepository.GetRoleByName(RoleNames.Seller);
            if (sellerRole is null)
            {
                return ApproveSellerApplicationResult.Failed($"Role '{RoleNames.Seller}' does not exist.");
            }

            // The role is granted before the application is marked approved. The two writes are not
            // atomic, and in this order a failure between them leaves the application pending, so an
            // administrator can approve it again (the grant below is idempotent).
            var rolesChanged = false;
            if (!user.Roles.Any(role => role.Id == sellerRole.Id))
            {
                user.Roles.Add(sellerRole);
                await userRepository.UpdateUser(user);
                rolesChanged = true;
            }

            application.Status = SellerApplicationStatus.Approved;
            application.DecidedOn = clock.UtcNow;
            application.DecidedBy = currentUserProvider.UserId;

            await applicationRepository.UpdateApplication(application);

            await events.Dispatch(new SellerApplicationApproved(application.Id, user.Id, clock.UtcNow), cancellationToken);

            if (rolesChanged)
            {
                await events.Dispatch(
                    new UserRolesChanged(user.Id, user.Roles.Select(r => r.Name).ToArray(), clock.UtcNow), cancellationToken);
            }

            return ApproveSellerApplicationResult.Succeeded(application.Id);
        }
    }
}
