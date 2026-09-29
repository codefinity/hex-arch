using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.CloseAccount;

namespace HexArch.Services.IdentityAccess.UseCases.CloseAccount
{
    public class CloseAccountCommandValidator : AbstractValidator<CloseAccountCommand>
    {
        public CloseAccountCommandValidator()
        {
            RuleFor(command => command.CurrentPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Current password is required.");
        }
    }
}
