using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestEmailVerification;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail;
using HexArch.Services.Specs.Support;
using HexArch.Services.Specs.Support.Fakes;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    // RequestPasswordReset, ResetPassword, RequestEmailVerification, VerifyEmail and ChangeEmail:
    // everything that proves control of a mailbox with an emailed code.
    [Binding]
    public class CredentialStepDefinitions
    {
        private readonly AccountContext context;

        public CredentialStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [Given(@"a password reset code ""([^""]*)"" was issued to ""([^""]*)"" that expires on ""([^""]*)""")]
        public void GivenAPasswordResetCodeWasIssuedTo(string code, string email, string utcTimestamp)
        {
            var user = context.Account(email);
            user.PasswordResetTokenHash = FakeSecureTokenGenerator.HashOf(code);
            user.PasswordResetTokenExpiresOn = AccountStepDefinitions.ParseUtc(utcTimestamp);
        }

        [Given(@"an email verification code ""([^""]*)"" was issued to ""([^""]*)"" that expires on ""([^""]*)""")]
        public void GivenAnEmailVerificationCodeWasIssuedTo(string code, string email, string utcTimestamp)
        {
            var user = context.Account(email);
            user.EmailVerificationTokenHash = FakeSecureTokenGenerator.HashOf(code);
            user.EmailVerificationTokenExpiresOn = AccountStepDefinitions.ParseUtc(utcTimestamp);
        }

        [Given(@"the account ""([^""]*)"" exists with an unverified email")]
        public void GivenTheAccountExistsWithAnUnverifiedEmail(string email) =>
            context.SeedAccount(email, new[] { RoleNames.Customer }).EmailVerified = false;

        [Given(@"the account ""([^""]*)"" has a pending email change to ""([^""]*)""")]
        public void GivenTheAccountHasAPendingEmailChangeTo(string email, string newEmail) =>
            context.Account(email).PendingEmail = newEmail;

        [Given(@"the account ""([^""]*)"" has been locked out by failed sign-ins")]
        public void GivenTheAccountHasBeenLockedOutByFailedSignIns(string email)
        {
            var user = context.Account(email);
            user.FailedSignInCount = 3;
            user.LockedOutUntil = context.Clock.UtcNow.AddMinutes(10);
        }

        [Given(@"the mail server is unavailable")]
        public void GivenTheMailServerIsUnavailable() => context.Emails.Unavailable = true;

        [When(@"a password reset is requested for ""([^""]*)""")]
        public async Task WhenAPasswordResetIsRequestedFor(string email)
        {
            var result = await context.RequestPasswordReset.Handle(new RequestPasswordResetCommand(email), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"""([^""]*)"" resets their password to ""([^""]*)"" using the code ""([^""]*)""")]
        public async Task WhenResetsTheirPasswordUsingTheCode(string email, string newPassword, string code)
        {
            var result = await context.ResetPassword.Handle(new ResetPasswordCommand(email, code, newPassword), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"I ask for an email verification code")]
        public async Task WhenIAskForAnEmailVerificationCode()
        {
            var result = await context.RequestEmailVerification.Handle(new RequestEmailVerificationCommand(), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"I verify my email with the code ""([^""]*)""")]
        public async Task WhenIVerifyMyEmailWithTheCode(string code)
        {
            var result = await context.VerifyEmail.Handle(new VerifyEmailCommand(code), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"I change my email to ""([^""]*)"" confirming with the password ""([^""]*)""")]
        public async Task WhenIChangeMyEmailToConfirmingWithThePassword(string newEmail, string password)
        {
            var result = await context.ChangeEmail.Handle(new ChangeEmailCommand(newEmail, password), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [Then(@"the account ""([^""]*)"" holds a password reset code that expires on ""([^""]*)""")]
        public void ThenTheAccountHoldsAPasswordResetCodeThatExpiresOn(string email, string utcTimestamp)
        {
            var user = context.Account(email);
            context.SecureTokens.LastIssued.ShouldNotBeNull();
            user.PasswordResetTokenHash.ShouldBe(FakeSecureTokenGenerator.HashOf(context.SecureTokens.LastIssued));
            user.PasswordResetTokenExpiresOn.ShouldBe(AccountStepDefinitions.ParseUtc(utcTimestamp));
        }

        [Then(@"the account ""([^""]*)"" holds no password reset code")]
        public void ThenTheAccountHoldsNoPasswordResetCode(string email)
        {
            var user = context.Account(email);
            user.PasswordResetTokenHash.ShouldBeNull();
            user.PasswordResetTokenExpiresOn.ShouldBeNull();
        }

        [Then(@"the account ""([^""]*)"" holds an email verification code that expires on ""([^""]*)""")]
        public void ThenTheAccountHoldsAnEmailVerificationCodeThatExpiresOn(string email, string utcTimestamp)
        {
            var user = context.Account(email);
            context.SecureTokens.LastIssued.ShouldNotBeNull();
            user.EmailVerificationTokenHash.ShouldBe(FakeSecureTokenGenerator.HashOf(context.SecureTokens.LastIssued));
            user.EmailVerificationTokenExpiresOn.ShouldBe(AccountStepDefinitions.ParseUtc(utcTimestamp));
        }

        [Then(@"the account ""([^""]*)"" holds no email verification code")]
        public void ThenTheAccountHoldsNoEmailVerificationCode(string email)
        {
            var user = context.Account(email);
            user.EmailVerificationTokenHash.ShouldBeNull();
            user.EmailVerificationTokenExpiresOn.ShouldBeNull();
        }

        [Then(@"the reset code is emailed to ""([^""]*)""")]
        public void ThenTheResetCodeIsEmailedTo(string email) => ShouldHaveEmailedTheLastCodeTo(email);

        [Then(@"the verification code is emailed to ""([^""]*)""")]
        public void ThenTheVerificationCodeIsEmailedTo(string email) => ShouldHaveEmailedTheLastCodeTo(email);

        [Then(@"the account ""([^""]*)"" is no longer locked out")]
        public void ThenTheAccountIsNoLongerLockedOut(string email)
        {
            var user = context.Account(email);
            user.LockedOutUntil.ShouldBeNull();
            user.FailedSignInCount.ShouldBe(0);
        }

        [Then(@"the email of ""([^""]*)"" is verified")]
        public void ThenTheEmailOfIsVerified(string email) => context.Account(email).EmailVerified.ShouldBeTrue();

        [Then(@"the email of ""([^""]*)"" is not verified")]
        public void ThenTheEmailOfIsNotVerified(string email) => context.Account(email).EmailVerified.ShouldBeFalse();

        [Then(@"the email address of ""([^""]*)"" is now ""([^""]*)""")]
        public void ThenTheEmailAddressOfIsNow(string email, string expected) => context.Account(email).Email.ShouldBe(expected);

        [Then(@"the account ""([^""]*)"" has a pending email change to ""([^""]*)""")]
        public void ThenTheAccountHasAPendingEmailChangeTo(string email, string newEmail) =>
            context.Account(email).PendingEmail.ShouldBe(newEmail);

        [Then(@"the account ""([^""]*)"" has no pending email change")]
        public void ThenTheAccountHasNoPendingEmailChange(string email) => context.Account(email).PendingEmail.ShouldBeNull();

        private void ShouldHaveEmailedTheLastCodeTo(string email)
        {
            context.SecureTokens.LastIssued.ShouldNotBeNull();
            var message = context.Emails.Sent.ShouldHaveSingleItem();
            message.To.ShouldBe(email);
            message.Body.ShouldContain(context.SecureTokens.LastIssued);
        }
    }
}
