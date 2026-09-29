using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;

namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser
{
    public class ShowUserResult
    {
        public bool Success { get; }
        public UserProfileReadModel? User { get; }
        public IReadOnlyList<string> Errors { get; }

        private ShowUserResult(bool success, UserProfileReadModel? user, IReadOnlyList<string> errors)
        {
            Success = success;
            User = user;
            Errors = errors;
        }

        public static ShowUserResult Succeeded(UserProfileReadModel user) =>
            new(true, user, Array.Empty<string>());

        public static ShowUserResult Failed(params string[] errors) =>
            new(false, null, errors);
    }
}
