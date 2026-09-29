using System.Globalization;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    [Binding]
    public class SignInStepDefinitions
    {
        private readonly SignInContext context;

        public SignInStepDefinitions(SignInContext context)
        {
            this.context = context;
        }

        [Given(@"the sign-in token expires on ""(.*)""")]
        public void GivenTheSignInTokenExpiresOn(string utcTimestamp) =>
            context.TokenGenerator.ExpiresOnUtc = ParseUtc(utcTimestamp);

        [Given(@"an active account exists for ""(.*)"" with password ""(.*)""")]
        public void GivenAnActiveAccountExistsForWithPassword(string email, string password) =>
            SeedAccount(email, password, active: true);

        [Given(@"an inactive account exists for ""(.*)"" with password ""(.*)""")]
        public void GivenAnInactiveAccountExistsForWithPassword(string email, string password) =>
            SeedAccount(email, password, active: false);

        [When(@"I sign in with email ""(.*)"" and password ""(.*)""")]
        public async Task WhenISignInWithEmailAndPassword(string email, string password)
        {
            var command = new SignInCommand(email, password);
            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [Then(@"the sign in succeeds")]
        public void ThenTheSignInSucceeds()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeTrue(string.Join("; ", context.Result.Errors));
        }

        [Then(@"the sign in fails with the error ""(.*)""")]
        public void ThenTheSignInFailsWithTheError(string error)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();
            context.Result.Errors.ShouldContain(error);
        }

        [Then(@"the sign in fails with these errors:")]
        public void ThenTheSignInFailsWithTheseErrors(DataTable table)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();

            var expected = table.Rows.Select(r => r["Error"]).ToArray();
            context.Result.Errors.ShouldBe(expected, ignoreOrder: true);
        }

        [Then(@"a token is returned")]
        public void ThenATokenIsReturned()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Token.ShouldNotBeNullOrEmpty();
        }

        [Then(@"no token is returned")]
        public void ThenNoTokenIsReturned()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Token.ShouldBeNull();
        }

        [Then(@"the token expires on ""(.*)""")]
        public void ThenTheTokenExpiresOn(string utcTimestamp)
        {
            context.Result.ShouldNotBeNull();
            context.Result.ExpiresOnUtc.ShouldBe(ParseUtc(utcTimestamp));
        }

        private void SeedAccount(string email, string password, bool active)
        {
            var hash = context.Hasher.Object.Hash(password);
            context.Users.Seed(new User
            {
                Id = Guid.NewGuid(),
                Name = "Existing User",
                Email = email,
                Password = hash.Hash,
                Salt = hash.Salt,
                MobileNo = "0000000000",
                Active = active,
                RegisteredOn = DateTime.UtcNow,
                Roles = new List<Role>()
            });
        }

        private static DateTime ParseUtc(string value) =>
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
