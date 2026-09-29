using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RejectSellerApplication;

namespace HexArch.Services.IdentityAccess.UseCases.RejectSellerApplication
{
    public class RejectSellerApplicationCommandValidator : AbstractValidator<RejectSellerApplicationCommand>
    {
        public RejectSellerApplicationCommandValidator()
        {
            RuleFor(command => command.ApplicationId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("ApplicationId is required.");

            RuleFor(command => command.Reason)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Reason is required.")
                .MaximumLength(500)
                .WithMessage("Reason must not exceed 500 characters.");
        }
    }
}
