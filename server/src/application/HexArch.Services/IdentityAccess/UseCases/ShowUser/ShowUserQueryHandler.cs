using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser;
using HexArch.Services.IdentityAccess.Ports.Output.Queries;

namespace HexArch.Services.IdentityAccess.UseCases.ShowUser
{
    /// <summary>Shows any user by id; the administrative counterpart of ShowUserProfile.</summary>
    public class ShowUserQueryHandler : IShowUserQueryHandler
    {
        private readonly IValidator<ShowUserQuery> validator;
        private readonly IUserProfileQuery userProfileQuery;

        public ShowUserQueryHandler(IValidator<ShowUserQuery> validator, IUserProfileQuery userProfileQuery)
        {
            this.validator = validator;
            this.userProfileQuery = userProfileQuery;
        }

        public async Task<ShowUserResult> Handle(ShowUserQuery query, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return ShowUserResult.Failed(errors);
            }

            var user = await userProfileQuery.GetUserProfile(query.UserId, cancellationToken);

            return user is null
                ? ShowUserResult.Failed("User not found.")
                : ShowUserResult.Succeeded(user);
        }
    }
}
