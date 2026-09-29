using HexArch.Services.IdentityAccess.Ports.Output.Queries.ReadModels;

namespace HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUserProfile
{
    public class ShowUserProfileResult
    {
        public bool Success { get; }
        public UserProfileReadModel? Profile { get; }
        public IReadOnlyList<string> Errors { get; }

        private ShowUserProfileResult(bool success, UserProfileReadModel? profile, IReadOnlyList<string> errors)
        {
            Success = success;
            Profile = profile;
            Errors = errors;
        }

        public static ShowUserProfileResult Succeeded(UserProfileReadModel profile) =>
            new(true, profile, Array.Empty<string>());

        public static ShowUserProfileResult Failed(params string[] errors) =>
            new(false, null, errors);
    }
}
