using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.SearchUsers;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession;
using HexArch.Services.IdentityAccess.UseCases.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.UseCases.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.UseCases.AssignRole;
using HexArch.Services.IdentityAccess.UseCases.ChangeEmail;
using HexArch.Services.IdentityAccess.UseCases.ChangePassword;
using HexArch.Services.IdentityAccess.UseCases.CloseAccount;
using HexArch.Services.IdentityAccess.UseCases.ReactivateUser;
using HexArch.Services.IdentityAccess.UseCases.RejectSellerApplication;
using HexArch.Services.IdentityAccess.UseCases.RequestEmailVerification;
using HexArch.Services.IdentityAccess.UseCases.RequestPasswordReset;
using HexArch.Services.IdentityAccess.UseCases.ResetPassword;
using HexArch.Services.IdentityAccess.UseCases.RevokeRole;
using HexArch.Services.IdentityAccess.UseCases.RevokeSessions;
using HexArch.Services.IdentityAccess.UseCases.SearchUsers;
using HexArch.Services.IdentityAccess.UseCases.ShowUser;
using HexArch.Services.IdentityAccess.UseCases.SignOut;
using HexArch.Services.IdentityAccess.UseCases.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.UseCases.ValidateSession;
using HexArch.Services.IdentityAccess.UseCases.VerifyEmail;
using HexArch.Services.Specs.Support.Fakes;

namespace HexArch.Services.Specs.Support
{
    // Injected fresh into each scenario by Reqnroll's context-injection container.
    //
    // Shared by every feature written after the first five, so that seeding accounts, signing in and
    // asserting on outcomes, events, sessions and emails is phrased once (AccountStepDefinitions)
    // rather than once per feature. Each feature's own step class only adds its When steps and the
    // Then steps specific to it.
    public class AccountContext
    {
        public const string DefaultPassword = "Passw0rd!";

        public AccountContext()
        {
            Roles.Add(RoleNames.Admin);
            Roles.Add(RoleNames.Customer);
            Roles.Add(RoleNames.Seller);
        }

        public InMemoryUserRepository Users { get; } = new();
        public InMemoryRoleRepository Roles { get; } = new();
        public InMemoryProfileRepository Profiles { get; } = new();
        public InMemorySellerApplicationRepository SellerApplications { get; } = new();
        public InMemoryUserProfileQuery UserProfiles { get; } = new();
        public InMemoryUserSearchQuery UserSearch { get; } = new();
        public FakeCurrentUserProvider CurrentUser { get; } = new();
        public FakePasswordHasher Hasher { get; } = new();
        public FakeJwtTokenGenerator Jwt { get; } = new();
        public FakeSecureTokenGenerator SecureTokens { get; } = new();
        public RecordingEmailSender Emails { get; } = new();
        public FixedSystemClock Clock { get; } = new();
        public RecordingEventDispatcher Events { get; } = new();

        // Keyed by the email an account was seeded with, so steps can keep naming it that way even
        // after a scenario changes or anonymises the address.
        public Dictionary<string, Guid> AccountIds { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<Guid, Guid> SeededSecurityStamps { get; } = new();

        // The outcome of the scenario's When step, whichever use case it ran.
        public bool? Succeeded { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; } = Array.Empty<string>();

        public void RecordOutcome(bool success, IReadOnlyList<string> errors)
        {
            Succeeded = success;
            Errors = errors;
        }

        public User SeedAccount(string email, IEnumerable<string> roleNames, bool active = true)
        {
            var hash = Hasher.Object.Hash(DefaultPassword);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Existing User",
                Email = email,
                Password = hash.Hash,
                Salt = hash.Salt,
                MobileNo = "0000000000",
                Active = active,
                RegisteredOn = Clock.UtcNow,
                SecurityStamp = Guid.NewGuid(),
                EmailVerified = true,
                Roles = roleNames.Select(Roles.Get).ToList()
            };

            Users.Seed(user);
            AccountIds[email] = user.Id;
            SeededSecurityStamps[user.Id] = user.SecurityStamp;

            return user;
        }

        public User Account(string seededEmail) => Users.Users.Single(user => user.Id == AccountIds[seededEmail]);

        public IReactivateUserCommandHandler ReactivateUser => new ReactivateUserCommandHandler(
            new ReactivateUserCommandValidator(), Users.Object, Clock.Object, Events.Object);

        public IAssignRoleCommandHandler AssignRole => new AssignRoleCommandHandler(
            new AssignRoleCommandValidator(), Users.Object, Roles.Object, Clock.Object, Events.Object);

        public IRevokeRoleCommandHandler RevokeRole => new RevokeRoleCommandHandler(
            new RevokeRoleCommandValidator(), Users.Object, Clock.Object, Events.Object);

        public ISignOutCommandHandler SignOut => new SignOutCommandHandler(
            CurrentUser.Object, Users.Object, Clock.Object, Events.Object);

        public IRevokeSessionsCommandHandler RevokeSessions => new RevokeSessionsCommandHandler(
            new RevokeSessionsCommandValidator(), Users.Object, Clock.Object, Events.Object);

        public IValidateSessionQueryHandler ValidateSession => new ValidateSessionQueryHandler(Users.Object);

        public IUpdateAccountDetailsCommandHandler UpdateAccountDetails => new UpdateAccountDetailsCommandHandler(
            new UpdateAccountDetailsCommandValidator(), CurrentUser.Object, Users.Object, Clock.Object, Events.Object);

        public IChangePasswordCommandHandler ChangePassword => new ChangePasswordCommandHandler(
            new ChangePasswordCommandValidator(), CurrentUser.Object, Users.Object, Hasher.Object, Jwt.Object, Clock.Object, Events.Object);

        public IRequestPasswordResetCommandHandler RequestPasswordReset => new RequestPasswordResetCommandHandler(
            new RequestPasswordResetCommandValidator(), Users.Object, SecureTokens.Object, Emails.Object, Clock.Object);

        public IResetPasswordCommandHandler ResetPassword => new ResetPasswordCommandHandler(
            new ResetPasswordCommandValidator(), Users.Object, SecureTokens.Object, Hasher.Object, Clock.Object, Events.Object);

        public IRequestEmailVerificationCommandHandler RequestEmailVerification => new RequestEmailVerificationCommandHandler(
            CurrentUser.Object, Users.Object, SecureTokens.Object, Emails.Object, Clock.Object);

        public IVerifyEmailCommandHandler VerifyEmail => new VerifyEmailCommandHandler(
            new VerifyEmailCommandValidator(), CurrentUser.Object, Users.Object, SecureTokens.Object, Clock.Object, Events.Object);

        public IChangeEmailCommandHandler ChangeEmail => new ChangeEmailCommandHandler(
            new ChangeEmailCommandValidator(), CurrentUser.Object, Users.Object, Hasher.Object, SecureTokens.Object, Emails.Object, Clock.Object);

        public ICloseAccountCommandHandler CloseAccount => new CloseAccountCommandHandler(
            new CloseAccountCommandValidator(), CurrentUser.Object, Users.Object, Profiles.Object, Hasher.Object, Clock.Object, Events.Object);

        public IApplyForSellerAccountCommandHandler ApplyForSellerAccount => new ApplyForSellerAccountCommandHandler(
            new ApplyForSellerAccountCommandValidator(), CurrentUser.Object, Users.Object, SellerApplications.Object, Clock.Object, Events.Object);

        public IApproveSellerApplicationCommandHandler ApproveSellerApplication => new ApproveSellerApplicationCommandHandler(
            new ApproveSellerApplicationCommandValidator(), CurrentUser.Object, SellerApplications.Object, Users.Object, Roles.Object, Clock.Object, Events.Object);

        public IRejectSellerApplicationCommandHandler RejectSellerApplication => new RejectSellerApplicationCommandHandler(
            new RejectSellerApplicationCommandValidator(), CurrentUser.Object, SellerApplications.Object, Clock.Object, Events.Object);

        public ISearchUsersQueryHandler SearchUsers => new SearchUsersQueryHandler(
            new SearchUsersQueryValidator(), UserSearch.Object);

        public IShowUserQueryHandler ShowUser => new ShowUserQueryHandler(
            new ShowUserQueryValidator(), UserProfiles.Object);
    }
}
