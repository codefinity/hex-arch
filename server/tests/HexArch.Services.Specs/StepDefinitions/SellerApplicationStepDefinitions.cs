using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication;
using HexArch.Services.Specs.Support;
using Reqnroll;
using Shouldly;

namespace HexArch.Services.Specs.StepDefinitions
{
    // ApplyForSellerAccount, ApproveSellerApplication and RejectSellerApplication.
    [Binding]
    public class SellerApplicationStepDefinitions
    {
        private readonly AccountContext context;

        public SellerApplicationStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [Given(@"the account ""([^""]*)"" has a pending seller application")]
        public void GivenTheAccountHasAPendingSellerApplication(string email) =>
            context.SellerApplications.Seed(new SellerApplication
            {
                Id = Guid.NewGuid(),
                UserId = context.AccountIds[email],
                BusinessName = "Existing Business",
                Status = SellerApplicationStatus.Pending,
                SubmittedOn = context.Clock.UtcNow.AddHours(-1)
            });

        [Given(@"the account ""([^""]*)"" had a seller application rejected")]
        public void GivenTheAccountHadASellerApplicationRejected(string email) =>
            context.SellerApplications.Seed(new SellerApplication
            {
                Id = Guid.NewGuid(),
                UserId = context.AccountIds[email],
                BusinessName = "Existing Business",
                Status = SellerApplicationStatus.Rejected,
                SubmittedOn = context.Clock.UtcNow.AddDays(-2),
                DecidedOn = context.Clock.UtcNow.AddDays(-1),
                DecisionNote = "Incomplete."
            });

        [When(@"I apply to become a seller trading as ""([^""]*)""")]
        public async Task WhenIApplyToBecomeASellerTradingAs(string businessName)
        {
            var result = await context.ApplyForSellerAccount.Handle(new ApplyForSellerAccountCommand(businessName), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        [When(@"the latest seller application from ""([^""]*)"" is approved")]
        public Task WhenTheLatestSellerApplicationFromIsApproved(string email) => Approve(LatestApplicationOf(email).Id);

        [When(@"an unknown seller application is approved")]
        public Task WhenAnUnknownSellerApplicationIsApproved() => Approve(Guid.NewGuid());

        [When(@"the latest seller application from ""([^""]*)"" is rejected because ""([^""]*)""")]
        public Task WhenTheLatestSellerApplicationFromIsRejectedBecause(string email, string reason) =>
            Reject(LatestApplicationOf(email).Id, reason);

        [When(@"an unknown seller application is rejected because ""([^""]*)""")]
        public Task WhenAnUnknownSellerApplicationIsRejectedBecause(string reason) => Reject(Guid.NewGuid(), reason);

        [Then(@"a pending seller application from ""([^""]*)"" trading as ""([^""]*)"" is stored")]
        public void ThenAPendingSellerApplicationFromTradingAsIsStored(string email, string businessName)
        {
            var application = LatestApplicationOf(email);
            application.Status.ShouldBe(SellerApplicationStatus.Pending);
            application.BusinessName.ShouldBe(businessName);
            application.SubmittedOn.ShouldBe(context.Clock.UtcNow);
        }

        [Then(@"the latest seller application from ""([^""]*)"" was ""([^""]*)"" by ""([^""]*)""")]
        public void ThenTheLatestSellerApplicationFromWasBy(string email, string status, string administrator)
        {
            var application = LatestApplicationOf(email);
            application.Status.ToString().ShouldBe(status);
            application.DecidedBy.ShouldBe(context.AccountIds[administrator]);
            application.DecidedOn.ShouldBe(context.Clock.UtcNow);
        }

        [Then(@"the latest seller application from ""([^""]*)"" notes ""([^""]*)""")]
        public void ThenTheLatestSellerApplicationFromNotes(string email, string note) =>
            LatestApplicationOf(email).DecisionNote.ShouldBe(note);

        private SellerApplication LatestApplicationOf(string email) =>
            context.SellerApplications.Applications.Last(application => application.UserId == context.AccountIds[email]);

        private async Task Approve(Guid applicationId)
        {
            var result = await context.ApproveSellerApplication.Handle(new ApproveSellerApplicationCommand(applicationId), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        private async Task Reject(Guid applicationId, string reason)
        {
            var result = await context.RejectSellerApplication.Handle(
                new RejectSellerApplicationCommand(applicationId, reason), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }
    }
}
