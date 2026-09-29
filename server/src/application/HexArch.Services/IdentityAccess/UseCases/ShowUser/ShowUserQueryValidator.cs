using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Queries.ShowUser;

namespace HexArch.Services.IdentityAccess.UseCases.ShowUser
{
    public class ShowUserQueryValidator : AbstractValidator<ShowUserQuery>
    {
        public ShowUserQueryValidator()
        {
            RuleFor(query => query.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");
        }
    }
}
