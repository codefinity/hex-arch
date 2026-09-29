using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ChangePassword;

namespace HexArch.Services.IdentityAccess.UseCases.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(command => command.CurrentPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Current password is required.");

            RuleFor(command => command.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password must be at least 8 characters long.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.")
                .NotEqual(command => command.CurrentPassword)
                .WithMessage("The new password must be different from the current password.");
        }
    }
}
