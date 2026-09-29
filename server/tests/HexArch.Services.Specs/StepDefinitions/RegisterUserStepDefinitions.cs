using System.Globalization;
using HexArch.Events.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    [Binding]
    public class RegisterUserStepDefinitions
    {
        private readonly RegistrationContext context;

        public RegisterUserStepDefinitions(RegistrationContext context)
        {
            this.context = context;
        }

        [Given(@"the default ""(.*)"" role is configured")]
        public void GivenTheDefaultRoleIsConfigured(string roleName) => context.Roles.Add(roleName);

        [Given(@"the default ""(.*)"" role is not configured")]
        public void GivenTheDefaultRoleIsNotConfigured(string roleName) => context.Roles.Remove(roleName);

        [Given(@"the system clock is fixed at ""(.*)""")]
        public void GivenTheSystemClockIsFixedAt(string utcTimestamp) =>
            context.Clock.UtcNow = ParseUtc(utcTimestamp);

        [Given(@"an account already exists for ""(.*)""")]
        public void GivenAnAccountAlreadyExistsFor(string email)
        {
            context.Users.Seed(new HexArch.Models.IdentityAccess.User
            {
                Id = Guid.NewGuid(),
                Name = "Existing User",
                Email = email,
                Password = "irrelevant-hash",
                Salt = "irrelevant-salt",
                MobileNo = "0000000000",
                Active = true,
                RegisteredOn = context.Clock.UtcNow,
                Roles = new List<HexArch.Models.IdentityAccess.Role>()
            });
        }

        [When(@"I register with the following details:")]
        public async Task WhenIRegisterWithTheFollowingDetails(DataTable table)
        {
            var row = table.Rows[0];
            var command = new RegisterUserCommand(
                row["Name"],
                row["Email"],
                row["Password"],
                row["MobileNo"]);

            context.UserCountBeforeAttempt = context.Users.Users.Count;
            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [Then(@"the registration succeeds")]
        public void ThenTheRegistrationSucceeds()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeTrue(string.Join("; ", context.Result.Errors));
        }

        [Then(@"the registration fails with the error ""(.*)""")]
        public void ThenTheRegistrationFailsWithTheError(string error)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();
            context.Result.Errors.ShouldContain(error);
        }

        [Then(@"the registration fails with these errors:")]
        public void ThenTheRegistrationFailsWithTheseErrors(DataTable table)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();

            var expected = table.Rows.Select(r => r["Error"]).ToArray();
            context.Result.Errors.ShouldBe(expected, ignoreOrder: true);
        }

        [Then(@"the new account is returned with an identifier")]
        public void ThenTheNewAccountIsReturnedWithAnIdentifier()
        {
            context.Result.ShouldNotBeNull();
            context.Result.UserId.ShouldNotBe(Guid.Empty);
        }

        [Then(@"an account exists for ""(.*)""")]
        public async Task ThenAnAccountExistsFor(string email)
        {
            var user = await context.Users.GetUser(email);
            user.ShouldNotBeNull();
        }

        [Then(@"that account is active")]
        public async Task ThenThatAccountIsActive()
        {
            var user = await RequireLastRegisteredUser();
            user.Active.ShouldBeTrue();
        }

        [Then(@"that account is registered on ""(.*)""")]
        public async Task ThenThatAccountIsRegisteredOn(string utcTimestamp)
        {
            var user = await RequireLastRegisteredUser();
            user.RegisteredOn.ShouldBe(ParseUtc(utcTimestamp));
        }

        [Then(@"that account is granted the ""(.*)"" role")]
        public async Task ThenThatAccountIsGrantedTheRole(string roleName)
        {
            var user = await RequireLastRegisteredUser();
            user.Roles.ShouldContain(role => role.Name == roleName);
        }

        [Then(@"that account is issued a security stamp")]
        public async Task ThenThatAccountIsIssuedASecurityStamp()
        {
            var user = await RequireLastRegisteredUser();
            user.SecurityStamp.ShouldNotBe(Guid.Empty);
        }

        [Then(@"the stored password for ""(.*)"" is not ""(.*)""")]
        public async Task ThenTheStoredPasswordForIsNot(string email, string plainTextPassword)
        {
            var user = await context.Users.GetUser(email);
            user.ShouldNotBeNull();
            user.Password.ShouldNotBe(plainTextPassword);
        }

        [Then(@"the stored password for ""(.*)"" is salted")]
        public async Task ThenTheStoredPasswordForIsSalted(string email)
        {
            var user = await context.Users.GetUser(email);
            user.ShouldNotBeNull();
            user.Salt.ShouldNotBeNullOrEmpty();
        }

        [Then(@"a ""UserRegistered"" event is published for the new account")]
        public void ThenAUserRegisteredEventIsPublishedForTheNewAccount()
        {
            context.Result.ShouldNotBeNull();
            var published = context.Events.Single<UserRegistered>();
            published.UserId.ShouldBe(context.Result.UserId);
        }

        [Then(@"that event occurred on ""(.*)""")]
        public void ThenThatEventOccurredOn(string utcTimestamp)
        {
            var published = context.Events.Single<UserRegistered>();
            published.OccurredOnUtc.ShouldBe(ParseUtc(utcTimestamp));
        }

        [Then(@"no new account is stored")]
        public void ThenNoNewAccountIsStored() => context.Users.Users.Count.ShouldBe(context.UserCountBeforeAttempt);

        [Then(@"no event is published")]
        public void ThenNoEventIsPublished() => context.Events.Published.ShouldBeEmpty();

        private async Task<HexArch.Models.IdentityAccess.User> RequireLastRegisteredUser()
        {
            context.Result.ShouldNotBeNull();
            var user = await context.Users.GetUser(context.Result.UserId);
            user.ShouldNotBeNull();
            return user;
        }

        private static DateTime ParseUtc(string value) =>
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
