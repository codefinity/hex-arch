using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;
using HexArch.Services.IdentityAccess.UseCases.DeactivateUser;
using HexArch.Services.Specs.Support.Fakes;

namespace HexArch.Services.Specs.Support
{
    // Injected fresh into each scenario by Reqnroll's context-injection container.
    public class DeactivationContext
    {
        public InMemoryUserRepository Users { get; } = new();

        public DeactivateUserResult? Result { get; set; }

        public IDeactivateUserCommandHandler Handler => new DeactivateUserCommandHandler(
            new DeactivateUserCommandValidator(), Users.Object);
    }
}
