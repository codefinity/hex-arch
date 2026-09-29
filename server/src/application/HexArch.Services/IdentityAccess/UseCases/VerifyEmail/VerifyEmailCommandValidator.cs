using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.VerifyEmail;

namespace HexArch.Services.IdentityAccess.UseCases.VerifyEmail
{
    public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        public VerifyEmailCommandValidator()
        {
            RuleFor(command => command.Token)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Verification token is required.");
        }
    }
}
