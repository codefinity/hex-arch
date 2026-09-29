using System.Globalization;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile;
using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    [Binding]
    public class ShowUserProfileStepDefinitions
    {
        private readonly ShowUserProfileContext context;

        public ShowUserProfileStepDefinitions(ShowUserProfileContext context)
        {
            this.context = context;
        }

        [Given(@"I am signed in as a user with a projected profile:")]
        public void GivenIAmSignedInAsAUserWithAProjectedProfile(DataTable table)
        {
            var row = table.Rows[0];
            var userId = Guid.NewGuid();

            context.UserProfiles.Seed(new UserProfileReadModel(
                userId,
                row["Name"],
                row["Email"],
                row["MobileNo"],
                true,
                DateTime.UtcNow,
                ParseRoles(row["Roles"]),
                NullIfEmpty(row["Bio"]),
                NullIfEmpty(row["Address"]),
                ParseDateOnly(row["DateOfBirth"]),
                NullIfEmpty(row["AvatarUrl"]),
                DateTime.UtcNow,
                DateTime.UtcNow));

            context.CurrentUser.UserId = userId;
        }

        [Given(@"I am signed in with no projected profile")]
        public void GivenIAmSignedInWithNoProjectedProfile() =>
            // No profile seeded for this id, simulating a signed-in session whose projection
            // has not landed yet (or an account that no longer exists).
            context.CurrentUser.UserId = Guid.NewGuid();

        [When(@"I view my profile")]
        public async Task WhenIViewMyProfile() =>
            context.Result = await context.Handler.Handle(new ShowUserProfileQuery(), CancellationToken.None);

        [Then(@"the profile is shown successfully")]
        public void ThenTheProfileIsShownSuccessfully()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeTrue(string.Join("; ", context.Result.Errors));
            context.Result.Profile.ShouldNotBeNull();
        }

        [Then(@"the profile is not found with the error ""(.*)""")]
        public void ThenTheProfileIsNotFoundWithTheError(string error)
        {
            context.Result.ShouldNotBeNull();
            context.Result.Success.ShouldBeFalse();
            context.Result.Errors.ShouldContain(error);
        }

        [Then(@"the shown profile's name is ""(.*)""")]
        public void ThenTheShownProfilesNameIs(string name) => RequireProfile().Name.ShouldBe(name);

        [Then(@"the shown profile's email is ""(.*)""")]
        public void ThenTheShownProfilesEmailIs(string email) => RequireProfile().Email.ShouldBe(email);

        [Then(@"the shown profile's mobile number is ""(.*)""")]
        public void ThenTheShownProfilesMobileNumberIs(string mobileNo) => RequireProfile().MobileNo.ShouldBe(mobileNo);

        [Then(@"the shown profile's bio is ""(.*)""")]
        public void ThenTheShownProfilesBioIs(string bio) => RequireProfile().Bio.ShouldBe(bio);

        [Then(@"the shown profile's bio is empty")]
        public void ThenTheShownProfilesBioIsEmpty() => RequireProfile().Bio.ShouldBeNull();

        [Then(@"the shown profile's address is ""(.*)""")]
        public void ThenTheShownProfilesAddressIs(string address) => RequireProfile().Address.ShouldBe(address);

        [Then(@"the shown profile's address is empty")]
        public void ThenTheShownProfilesAddressIsEmpty() => RequireProfile().Address.ShouldBeNull();

        [Then(@"the shown profile's date of birth is ""(.*)""")]
        public void ThenTheShownProfilesDateOfBirthIs(string date) => RequireProfile().DateOfBirth.ShouldBe(ParseUtc(date));

        [Then(@"the shown profile's date of birth is empty")]
        public void ThenTheShownProfilesDateOfBirthIsEmpty() => RequireProfile().DateOfBirth.ShouldBeNull();

        [Then(@"the shown profile's avatar URL is ""(.*)""")]
        public void ThenTheShownProfilesAvatarUrlIs(string avatarUrl) => RequireProfile().AvatarUrl.ShouldBe(avatarUrl);

        [Then(@"the shown profile's avatar URL is empty")]
        public void ThenTheShownProfilesAvatarUrlIsEmpty() => RequireProfile().AvatarUrl.ShouldBeNull();

        [Then(@"the shown profile's roles are ""(.*)""")]
        public void ThenTheShownProfilesRolesAre(string roles) =>
            RequireProfile().Roles.Select(role => role.Name).ShouldBe(ParseRoleNames(roles), ignoreOrder: true);

        private UserProfileReadModel RequireProfile()
        {
            context.Result.ShouldNotBeNull();
            context.Result.Profile.ShouldNotBeNull();
            return context.Result.Profile;
        }

        private static IReadOnlyList<RoleReadModel> ParseRoles(string value) =>
            ParseRoleNames(value).Select(name => new RoleReadModel(Guid.NewGuid(), name)).ToArray();

        private static IEnumerable<string> ParseRoleNames(string value) =>
            value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;

        private static DateTime? ParseDateOnly(string value) => string.IsNullOrEmpty(value) ? null : ParseUtc(value);

        private static DateTime ParseUtc(string value) =>
            DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
    }
}
