using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeSessions;

namespace HexArch.Services.IdentityAccess.UseCases.RevokeSessions
{
    public class RevokeSessionsCommandValidator : AbstractValidator<RevokeSessionsCommand>
    {
        public RevokeSessionsCommandValidator()
        {
            RuleFor(command => command.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("UserId is required.");
        }
    }
}
