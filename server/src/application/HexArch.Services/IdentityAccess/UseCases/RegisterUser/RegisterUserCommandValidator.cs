using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RegisterUser;

namespace HexArch.Services.IdentityAccess.UseCases.RegisterUser
{
    public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(command => command.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Name is required.");

            RuleFor(command => command.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("A valid email is required.")
                .EmailAddress()
                .WithMessage("A valid email is required.");

            RuleFor(command => command.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password must be at least 8 characters long.")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.");

            RuleFor(command => command.MobileNo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mobile number is required.");
        }
    }
}
