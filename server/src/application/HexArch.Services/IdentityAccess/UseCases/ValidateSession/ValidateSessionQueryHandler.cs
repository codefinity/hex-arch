using HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.ValidateSession
{
    /// <summary>
    /// Decides whether a token that is otherwise valid (signature, lifetime) still represents a live
    /// session. Called on every authenticated request, so it reads the write model directly: the
    /// projection may lag, and a revoked session must stop working immediately.
    /// </summary>
    public class ValidateSessionQueryHandler : IValidateSessionQueryHandler
    {
        private readonly IUserRepository userRepository;

        public ValidateSessionQueryHandler(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<ValidateSessionResult> Handle(ValidateSessionQuery query, CancellationToken cancellationToken)
        {
            var user = await userRepository.GetUser(query.UserId);

            if (user is null || !user.Active || user.SecurityStamp != query.SecurityStamp)
            {
                return ValidateSessionResult.Failed("The session is no longer valid.");
            }

            return ValidateSessionResult.Succeeded();
        }
    }
}
