namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile
{
    public interface IShowUserProfileQueryHandler
    {
        Task<ShowUserProfileResult> Handle(ShowUserProfileQuery query, CancellationToken cancellationToken);
    }
}
