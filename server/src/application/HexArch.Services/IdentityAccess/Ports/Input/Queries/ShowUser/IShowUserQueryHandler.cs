namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser
{
    public interface IShowUserQueryHandler
    {
        Task<ShowUserResult> Handle(ShowUserQuery query, CancellationToken cancellationToken);
    }
}
