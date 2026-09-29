namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ValidateSession
{
    public interface IValidateSessionQueryHandler
    {
        Task<ValidateSessionResult> Handle(ValidateSessionQuery query, CancellationToken cancellationToken);
    }
}
