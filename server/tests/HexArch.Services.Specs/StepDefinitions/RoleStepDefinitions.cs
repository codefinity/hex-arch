using HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole;
using HexArch.Services.Specs.Support;
using Reqnroll;

namespace HexArch.Services.Specs.StepDefinitions
{
    // AssignRole and RevokeRole.
    [Binding]
    public class RoleStepDefinitions
    {
        private readonly AccountContext context;

        public RoleStepDefinitions(AccountContext context)
        {
            this.context = context;
        }

        [When(@"the ""([^""]*)"" role is assigned to ""([^""]*)""")]
        public Task WhenTheRoleIsAssignedTo(string roleName, string email) => Assign(context.AccountIds[email], roleName);

        [When(@"the ""([^""]*)"" role is assigned to an unknown account")]
        public Task WhenTheRoleIsAssignedToAnUnknownAccount(string roleName) => Assign(Guid.NewGuid(), roleName);

        [When(@"the ""([^""]*)"" role is revoked from ""([^""]*)""")]
        public Task WhenTheRoleIsRevokedFrom(string roleName, string email) => Revoke(context.AccountIds[email], roleName);

        [When(@"the ""([^""]*)"" role is revoked from an unknown account")]
        public Task WhenTheRoleIsRevokedFromAnUnknownAccount(string roleName) => Revoke(Guid.NewGuid(), roleName);

        private async Task Assign(Guid userId, string roleName)
        {
            var result = await context.AssignRole.Handle(new AssignRoleCommand(userId, roleName), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }

        private async Task Revoke(Guid userId, string roleName)
        {
            var result = await context.RevokeRole.Handle(new RevokeRoleCommand(userId, roleName), CancellationToken.None);
            context.RecordOutcome(result.Success, result.Errors);
        }
    }
}
