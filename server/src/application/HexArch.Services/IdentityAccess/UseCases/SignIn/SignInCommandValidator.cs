using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;

namespace HexArch.Services.IdentityAccess.UseCases.SignIn
{
    public class SignInCommandValidator : AbstractValidator<SignInCommand>
    {
        public SignInCommandValidator()
        {
            RuleFor(command => command.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("A valid email is required.")
                .EmailAddress()
                .WithMessage("A valid email is required.");

            RuleFor(command => command.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}
