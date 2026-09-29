using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ResetPassword;

namespace HexArch.Services.IdentityAccess.UseCases.ResetPassword
{
    public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordCommandValidator()
        {
            RuleFor(command => command.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("A valid email is required.")
                .EmailAddress()
                .WithMessage("A valid email is required.");

            RuleFor(command => command.Token)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Reset token is required.");

            RuleFor(command => command.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password must be at least 8 characters long.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.");
        }
    }
}
