using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RequestPasswordReset;

namespace HexArch.Services.IdentityAccess.UseCases.RequestPasswordReset
{
    public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
    {
        public RequestPasswordResetCommandValidator()
        {
            RuleFor(command => command.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("A valid email is required.")
                .EmailAddress()
                .WithMessage("A valid email is required.");
        }
    }
}
