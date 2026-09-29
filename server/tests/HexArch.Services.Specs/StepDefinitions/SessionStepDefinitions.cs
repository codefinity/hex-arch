using HexArch.Services.IdentityAccess.Ports.Input.Commands.ReactivateUser;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignOut;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    // ReactivateUser, SignOut, RevokeSessions and ValidateSession: whether an account may be used.
    [Binding]
    public class SessionStepDefinitions
    {
        private readonly AccountContext context;

        public SessionStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [When(@"the account ""([^""]*)"" is reactivated because ""([^""]*)""")]
        public Task WhenTheAccountIsReactivatedBecause(string email, string reason) =>
            Reactivate(new ReactivateUserCommand(context.AccountIds[email], reason));

        [When(@"the account ""([^""]*)"" is reactivated because ""([^""]*)"" only if it was deactivated for ""([^""]*)""")]
        public Task WhenTheAccountIsReactivatedBecauseOnlyIfItWasDeactivatedFor(string email, string reason, string deactivationReason) =>
            Reactivate(new ReactivateUserCommand(context.AccountIds[email], reason, deactivationReason));

        [When(@"an unknown account is reactivated because ""([^""]*)""")]
        public Task WhenAnUnknownAccountIsReactivatedBecause(string reason) =>
            Reactivate(new ReactivateUserCommand(Guid.NewGuid(), reason));

        [Then(@"the account ""([^""]*)"" no longer records a deactivation")]
        public void ThenTheAccountNoLongerRecordsADeactivation(string email)
        {
            var user = context.Account(email);
            user.DeactivationReason.ShouldBeNull();
            user.DeactivatedOn.ShouldBeNull();
        }

        [When(@"I sign out")]
        public async Task WhenISignOut()
        {
            var result = await context.SignOut.Handle(new SignOutCommand(), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"an administrator revokes every session of ""([^""]*)""")]
        public Task WhenAnAdministratorRevokesEverySessionOf(string email) => RevokeSessions(context.AccountIds[email]);

        [When(@"an administrator revokes every session of an unknown account")]
        public Task WhenAnAdministratorRevokesEverySessionOfAnUnknownAccount() => RevokeSessions(Guid.NewGuid());

        [When(@"a session for ""([^""]*)"" presents the current security stamp")]
        public Task WhenASessionForPresentsTheCurrentSecurityStamp(string email)
        {
            var user = context.Account(email);
            return Validate(user.Id, user.SecurityStamp);
        }

        [When(@"a session for ""([^""]*)"" presents an outdated security stamp")]
        public Task WhenASessionForPresentsAnOutdatedSecurityStamp(string email) =>
            Validate(context.AccountIds[email], Guid.NewGuid());

        [When(@"a session for an unknown account is validated")]
        public Task WhenASessionForAnUnknownAccountIsValidated() => Validate(Guid.NewGuid(), Guid.NewGuid());

        private async Task Reactivate(ReactivateUserCommand command)
        {
            var result = await context.ReactivateUser.Handle(command, CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        private async Task RevokeSessions(Guid userId)
        {
            var result = await context.RevokeSessions.Handle(new RevokeSessionsCommand(userId), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        private async Task Validate(Guid userId, Guid securityStamp)
        {
            var result = await context.ValidateSession.Handle(new ValidateSessionQuery(userId, securityStamp), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }
    }
}
