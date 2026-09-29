using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangeEmail;

namespace HexArch.Services.IdentityAccess.UseCases.ChangeEmail
{
    public class ChangeEmailCommandValidator : AbstractValidator<ChangeEmailCommand>
    {
        public ChangeEmailCommandValidator()
        {
            RuleFor(command => command.NewEmail)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("A valid email is required.")
                .EmailAddress()
                .WithMessage("A valid email is required.")
                // users.users.email is VARCHAR(50).
                .MaximumLength(50)
                .WithMessage("Email must not exceed 50 characters.");

            RuleFor(command => command.CurrentPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Current password is required.");
        }
    }
}
