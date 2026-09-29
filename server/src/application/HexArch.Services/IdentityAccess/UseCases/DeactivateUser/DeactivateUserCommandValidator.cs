using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.DeactivateUser;

namespace HexArch.Services.IdentityAccess.UseCases.DeactivateUser
{
    public class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
    {
        public DeactivateUserCommandValidator()
        {
            RuleFor(command => command.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(command => command.Reason)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Reason is required.");
        }
    }
}
