using System.Globalization;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    [Binding]
    public class UpdateProfileStepDefinitions
    {
        private readonly UpdateProfileContext context;

        public UpdateProfileStepDefinitions(UpdateProfileContext context)
        {
            this.context = context;
        }

        [Given(@"the profile clock is fixed at ""(.*)""")]
        public void GivenTheProfileClockIsFixedAt(string utcTimestamp) =>
            context.Clock.UtcNow = ParseUtc(utcTimestamp);

        [Given(@"a user account exists for ""(.*)""")]
        public void GivenAUserAccountExistsFor(string email)
        {
            context.Users.Seed(new User
            {
                Id = Guid.NewGuid(),
                Name = "Existing User",
                Email = email,
                Password = "irrelevant-hash",
                Salt = "irrelevant-salt",
                MobileNo = "0000000000",
                Active = true,
                RegisteredOn = context.Clock.UtcNow,
                Roles = new List<Role>()
            });
        }

        [Given(@"I am signed in as ""(.*)""")]
        public async Task GivenIAmSignedInAs(string email)
        {
            var user = await context.Users.GetUser(email);
            user.ShouldNotBeNull();
            context.CurrentUser.UserId = user.Id;
        }

        [Given(@"my profile already has the bio ""(.*)""")]
        public void GivenMyProfileAlreadyHasTheBio(string bio) =>
            context.Profiles.Seed(new Profile
            {
                UserId = context.CurrentUser.UserId,
                Bio = bio,
                UpdatedOn = context.Clock.UtcNow
            });

        [Given(@"my account has been removed")]
        public void GivenMyAccountHasBeenRemoved() =>
            // Point the current user at an id nothing in the repository was seeded with,
            // simulating a signed-in session for an account that no longer exists.
            context.CurrentUser.UserId = Guid.NewGuid();

        [When(@"I update my profile with the following details:")]
        public async Task WhenIUpdateMyProfileWithTheFollowingDetails(DataTable table)
        {
            var row = table.Rows[0];
            var command = new UpdateProfileCommand(
                NullIfEmpty(row["Bio"]),
                NullIfEmpty(row["Address"]),
                ParseDateOnly(row["DateOfBirth"]),
                NullIfEmpty(row["AvatarUrl"]));

            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [When(@"I update my profile with a bio of (\d+) characters")]
        public async Task WhenIUpdateMyProfileWithABioOfCharacters(int length)
        {
            var command = new UpdateProfileCommand(new string('a', length), null, null, null);
            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [When(@"I update my profile with an address of (\d+) characters")]
        public async Task WhenIUpdateMyProfileWithAnAddressOfCharacters(int length)
        {
            var command = new UpdateProfileCommand(null, new string('a', length), null, null);
            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [When(@"I update my profile with a bio of (\d+) characters, an address of (\d+) characters, an avatar URL of ""(.*)"", and a date of birth of ""(.*)""")]
        public async Task WhenIUpdateMyProfileWithAllInvalidDetails(int bioLength, int addressLength, string avatarUrl, string dateOfBirth)
        {
            var command = new UpdateProfileCommand(
                new string('a', bioLength),
                new string('a', addressLength),
                ParseUtc(dateOfBirth),
                avatarUrl);

            context.Result = await context.Handler.Handle(command, CancellationToken.None);
        }

        [Then(@"the profile update succeeds")]
        public void ThenTheProfileUpdateSucceeds()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeTrue(string.Join("; ", context.Result.Errors));
        }

        [Then(@"the profile update fails with the error ""(.*)""")]
        public void ThenTheProfileUpdateFailsWithTheError(string error)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();
            context.Result.Errors.ShouldContain(error);
        }

        [Then(@"the profile update fails with these errors:")]
        public void ThenTheProfileUpdateFailsWithTheseErrors(DataTable table)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();

            var expected = table.Rows.Select(r => r["Error"]).ToArray();
            context.Result.Errors.ShouldBe(expected, ignoreOrder: true);
        }

        [Then(@"my profile bio is ""(.*)""")]
        public async Task ThenMyProfileBioIs(string bio)
        {
            var profile = await RequireMyProfile();
            profile.Bio.ShouldBe(bio);
        }

        [Then(@"my profile address is ""(.*)""")]
        public async Task ThenMyProfileAddressIs(string address)
        {
            var profile = await RequireMyProfile();
            profile.Address.ShouldBe(address);
        }

        [Then(@"my profile date of birth is ""(.*)""")]
        public async Task ThenMyProfileDateOfBirthIs(string date)
        {
            var profile = await RequireMyProfile();
            profile.DateOfBirth.ShouldBe(ParseUtc(date));
        }

        [Then(@"my profile avatar URL is ""(.*)""")]
        public async Task ThenMyProfileAvatarUrlIs(string avatarUrl)
        {
            var profile = await RequireMyProfile();
            profile.AvatarUrl.ShouldBe(avatarUrl);
        }

        [Then(@"my profile was last updated on ""(.*)""")]
        public async Task ThenMyProfileWasLastUpdatedOn(string utcTimestamp)
        {
            var profile = await RequireMyProfile();
            profile.UpdatedOn.ShouldBe(ParseUtc(utcTimestamp));
        }

        [Then(@"a ""UserProfileUpdated"" event is published for my account")]
        public void ThenAUserProfileUpdatedEventIsPublishedForMyAccount()
        {
            context.Result.ShouldNotBeNull();
            var published = context.Events.Single<UserProfileUpdated>();
            published.UserId.ShouldBe(context.Result.UserId);
        }

        [Then(@"the profile update event occurred on ""(.*)""")]
        public void ThenTheProfileUpdateEventOccurredOn(string utcTimestamp)
        {
            var published = context.Events.Single<UserProfileUpdated>();
            published.OccurredOnUtc.ShouldBe(ParseUtc(utcTimestamp));
        }

        [Then(@"no profile event is published")]
        public void ThenNoProfileEventIsPublished() => context.Events.Published.ShouldBeEmpty();

        private async Task<Profile> RequireMyProfile()
        {
            var profile = await context.Profiles.GetProfile(context.CurrentUser.UserId);
            profile.ShouldNotBeNull();
            return profile;
        }

        private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        private static DateTime? ParseDateOnly(string value) => string.IsNullOrEmpty(value) ? null : ParseUtc(value);

        private static DateTime ParseUtc(string value) =>
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
