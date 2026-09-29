using System.Globalization;
using HexArch.Models.IdentityAccess;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    // Steps shared by every feature that uses AccountContext. Captures are ([^"]*) rather than (.*)
    // so these patterns can never also match a longer step from another feature.
    [Binding]
    public class AccountStepDefinitions
    {
        private readonly AccountContext context;

        public AccountStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [Given(@"the clock reads ""([^""]*)""")]
        public void GivenTheClockReads(string utcTimestamp) => context.Clock.UtcNow = ParseUtc(utcTimestamp);

        [Given(@"the account ""([^""]*)"" exists")]
        public void GivenTheAccountExists(string email) => context.SeedAccount(email, new[] { RoleNames.Customer });

        [Given(@"the account ""([^""]*)"" exists with the roles ""([^""]*)""")]
        public void GivenTheAccountExistsWithTheRoles(string email, string roles) =>
            context.SeedAccount(email, ParseList(roles));

        [Given(@"the account ""([^""]*)"" was deactivated for ""([^""]*)""")]
        public void GivenTheAccountWasDeactivatedFor(string email, string reason)
        {
            var user = context.SeedAccount(email, new[] { RoleNames.Customer }, active: false);
            user.DeactivationReason = reason;
            user.DeactivatedOn = context.Clock.UtcNow;
        }

        [Given(@"the account ""([^""]*)"" was closed")]
        public void GivenTheAccountWasClosed(string email)
        {
            var user = context.SeedAccount(email, new[] { RoleNames.Customer }, active: false);
            user.DeactivationReason = "Account closed.";
            user.ClosedOn = context.Clock.UtcNow;
        }

        [Given(@"I am signed in to the account ""([^""]*)""")]
        public void GivenIAmSignedInToTheAccount(string email) => context.CurrentUser.UserId = context.AccountIds[email];

        [Given(@"the signed-in account no longer exists")]
        public void GivenTheSignedInAccountNoLongerExists() =>
            // An id nothing was seeded with: a session for an account that has since gone.
            context.CurrentUser.UserId = Guid.NewGuid();

        [Then(@"the request succeeds")]
        public void ThenTheRequestSucceeds()
        {
            context.Succeeded.ShouldNotBeNull("No When step recorded an outcome.");
            context.Succeeded.Value.ShouldBeTrue(string.Join("; ", context.Errors));
        }

        [Then(@"the request fails with the error ""([^""]*)""")]
        public void ThenTheRequestFailsWithTheError(string error)
        {
            context.Succeeded.ShouldNotBeNull("No When step recorded an outcome.");
            context.Succeeded.Value.ShouldBeFalse();
            context.Errors.ShouldContain(error);
        }

        [Then(@"the request fails with these errors:")]
        public void ThenTheRequestFailsWithTheseErrors(DataTable table)
        {
            context.Succeeded.ShouldNotBeNull("No When step recorded an outcome.");
            context.Succeeded.Value.ShouldBeFalse();
            context.Errors.ShouldBe(table.Rows.Select(row => row["Error"]).ToArray(), ignoreOrder: true);
        }

        [Then(@"the account ""([^""]*)"" is active")]
        public void ThenTheAccountIsActive(string email) => context.Account(email).Active.ShouldBeTrue();

        [Then(@"the account ""([^""]*)"" is inactive")]
        public void ThenTheAccountIsInactive(string email) => context.Account(email).Active.ShouldBeFalse();

        [Then(@"the account ""([^""]*)"" holds the roles ""([^""]*)""")]
        public void ThenTheAccountHoldsTheRoles(string email, string roles) =>
            context.Account(email).Roles.Select(role => role.Name).ShouldBe(ParseList(roles), ignoreOrder: true);

        [Then(@"every session of ""([^""]*)"" is revoked")]
        public void ThenEverySessionOfIsRevoked(string email)
        {
            var user = context.Account(email);
            user.SecurityStamp.ShouldNotBe(context.SeededSecurityStamps[user.Id]);
        }

        [Then(@"the sessions of ""([^""]*)"" are left alone")]
        public void ThenTheSessionsOfAreLeftAlone(string email)
        {
            var user = context.Account(email);
            user.SecurityStamp.ShouldBe(context.SeededSecurityStamps[user.Id]);
        }

        [Then(@"a ""([^""]*)"" event is published for the account ""([^""]*)""")]
        public void ThenAnEventIsPublishedForTheAccount(string eventName, string email)
        {
            var matching = context.Events.Published.Where(e => e.GetType().Name == eventName).ToList();
            matching.Count.ShouldBe(1, $"Expected exactly one {eventName}; published: {DescribePublished()}");

            var published = matching[0];
            published.GetType().GetProperty("UserId")!.GetValue(published).ShouldBe(context.AccountIds[email]);
            published.OccurredOnUtc.ShouldBe(context.Clock.UtcNow);
        }

        [Then(@"no events are published")]
        public void ThenNoEventsArePublished() =>
            context.Events.Published.ShouldBeEmpty($"Published: {DescribePublished()}");

        [Then(@"an email is sent to ""([^""]*)""")]
        public void ThenAnEmailIsSentTo(string email) =>
            context.Emails.Sent.ShouldContain(message => message.To == email);

        [Then(@"no email is sent")]
        public void ThenNoEmailIsSent() => context.Emails.Sent.ShouldBeEmpty();

        private string DescribePublished() =>
            string.Join(", ", context.Events.Published.Select(e => e.GetType().Name).DefaultIfEmpty("nothing"));

        internal static IEnumerable<string> ParseList(string value) =>
            value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        internal static DateTime ParseUtc(string value) =>
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
