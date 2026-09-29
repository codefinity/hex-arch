using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    [Binding]
    public class DeactivateUserStepDefinitions
    {
        private readonly DeactivationContext context;
        private Guid userId;
        private int userCountBeforeAttempt;

        public DeactivateUserStepDefinitions(DeactivationContext context)
        {
            this.context = context;
        }

        [Given(@"the account ""(.*)"" is currently active")]
        public void GivenTheAccountIsCurrentlyActive(string email) => SeedAccount(email, active: true);

        [Given(@"the account ""(.*)"" is currently inactive")]
        public void GivenTheAccountIsCurrentlyInactive(string email) => SeedAccount(email, active: false);

        [Given(@"there is no account for the reported user")]
        public void GivenThereIsNoAccountForTheReportedUser() => userId = Guid.NewGuid();

        [When(@"the account is deactivated with reason ""(.*)""")]
        public async Task WhenTheAccountIsDeactivatedWithReason(string reason)
        {
            userCountBeforeAttempt = context.Users.Users.Count;
            var command = new DeactivateUserCommand(userId, reason);
            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [Then(@"the deactivation succeeds")]
        public void ThenTheDeactivationSucceeds()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeTrue(string.Join("; ", context.Result.Errors));
        }

        [Then(@"the deactivation fails with the error ""(.*)""")]
        public void ThenTheDeactivationFailsWithTheError(string error)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();
            context.Result.Errors.ShouldContain(error);
        }

        [Then(@"the account for ""(.*)"" is no longer active")]
        public async Task ThenTheAccountForIsNoLongerActive(string email)
        {
            var user = await context.Users.GetUser(email);
            user.ShouldNotBeNull();
            user.Active.ShouldBeFalse();
        }

        [Then(@"no account is stored again")]
        public void ThenNoAccountIsStoredAgain() => context.Users.Users.Count.ShouldBe(userCountBeforeAttempt);

        private void SeedAccount(string email, bool active)
        {
            userId = Guid.NewGuid();
            context.Users.Seed(new HexArch.Models.IdentityAccess.User
            {
                Id = userId,
                Name = "Existing User",
                Email = email,
                Password = "irrelevant-hash",
                Salt = "irrelevant-salt",
                MobileNo = "0000000000",
                Active = active,
                RegisteredOn = DateTime.UtcNow,
                Roles = new List<HexArch.Models.IdentityAccess.Role>()
            });
        }
    }
}
