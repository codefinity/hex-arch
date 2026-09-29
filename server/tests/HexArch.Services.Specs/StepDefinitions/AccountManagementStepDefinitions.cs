using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails;
using HexArch.Services.IdentityAccess.UseCases.CloseAccount;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    // UpdateAccountDetails, ChangePassword and CloseAccount: the signed-in user managing their account.
    [Binding]
    public class AccountManagementStepDefinitions
    {
        private readonly AccountContext context;
        private ChangePasswordResult? changePasswordResult;

        public AccountManagementStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [Given(@"the account ""([^""]*)"" has a profile with the bio ""([^""]*)""")]
        public void GivenTheAccountHasAProfileWithTheBio(string email, string bio) =>
            context.Profiles.Seed(new Profile { UserId = context.AccountIds[email], Bio = bio, UpdatedOn = context.Clock.UtcNow });

        [When(@"I change my name to ""([^""]*)"" and my mobile number to ""([^""]*)""")]
        public async Task WhenIChangeMyNameAndMyMobileNumber(string name, string mobileNo)
        {
            var result = await context.UpdateAccountDetails.Handle(new UpdateAccountDetailsCommand(name, mobileNo), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"I change my name to one of (\d+) characters and my mobile number to one of (\d+) characters")]
        public Task WhenIChangeMyNameAndMyMobileNumberToLongValues(int nameLength, int mobileNoLength) =>
            WhenIChangeMyNameAndMyMobileNumber(new string('a', nameLength), new string('1', mobileNoLength));

        [Then(@"the account ""([^""]*)"" is named ""([^""]*)"" with the mobile number ""([^""]*)""")]
        public void ThenTheAccountIsNamedWithTheMobileNumber(string email, string name, string mobileNo)
        {
            var user = context.Account(email);
            user.Name.ShouldBe(name);
            user.MobileNo.ShouldBe(mobileNo);
        }

        [When(@"I change my password from ""([^""]*)"" to ""([^""]*)""")]
        public async Task WhenIChangeMyPasswordFromTo(string currentPassword, string newPassword)
        {
            changePasswordResult = await context.ChangePassword.Handle(
                new ChangePasswordCommand(currentPassword, newPassword), CancellationToken.None);
            context.RecordOutcome(changePasswordResult.Success, changePasswordResult.Errors);
        }

        [Then(@"the password of ""([^""]*)"" is now ""([^""]*)""")]
        public void ThenThePasswordOfIsNow(string email, string password)
        {
            var user = context.Account(email);
            context.Hasher.Object.Verify(password, user.Password, user.Salt).ShouldBeTrue();
        }

        [Then(@"a fresh token is returned for ""([^""]*)""")]
        public void ThenAFreshTokenIsReturnedFor(string email)
        {
            changePasswordResult.ShouldNotBeNull();
            // FakeJwtTokenGenerator encodes the user id in the token.
            changePasswordResult.Token.ShouldBe($"token-for:{context.AccountIds[email]}");
        }

        [When(@"I close my account confirming with the password ""([^""]*)""")]
        public async Task WhenICloseMyAccountConfirmingWithThePassword(string password)
        {
            var result = await context.CloseAccount.Handle(new CloseAccountCommand(password), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [Then(@"the account ""([^""]*)"" is closed and anonymised")]
        public void ThenTheAccountIsClosedAndAnonymised(string email)
        {
            var user = context.Account(email);
            user.ClosedOn.ShouldBe(context.Clock.UtcNow);
            user.Name.ShouldBe(CloseAccountCommandHandler.ClosedAccountName);
            user.Email.ShouldNotContain(email);
            user.Email.ShouldEndWith("@closed.invalid");
            user.Email.Length.ShouldBeLessThanOrEqualTo(50);
            user.MobileNo.ShouldBeEmpty();
            context.Hasher.Object.Verify(AccountContext.DefaultPassword, user.Password, user.Salt).ShouldBeFalse();
        }

        [Then(@"the account ""([^""]*)"" has no profile")]
        public void ThenTheAccountHasNoProfile(string email) =>
            context.Profiles.Profiles.ShouldNotContain(profile => profile.UserId == context.AccountIds[email]);

        [Then(@"no account uses the email ""([^""]*)""")]
        public async Task ThenNoAccountUsesTheEmail(string email) =>
            (await context.Users.GetUser(email)).ShouldBeNull();
    }
}
