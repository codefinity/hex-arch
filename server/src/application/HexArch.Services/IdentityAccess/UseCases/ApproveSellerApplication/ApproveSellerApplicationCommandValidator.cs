using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApproveSellerApplication;

namespace HexArch.Services.IdentityAccess.UseCases.ApproveSellerApplication
{
    public class ApproveSellerApplicationCommandValidator : AbstractValidator<ApproveSellerApplicationCommand>
    {
        public ApproveSellerApplicationCommandValidator()
        {
            RuleFor(command => command.ApplicationId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("ApplicationId is required.");
        }
    }
}
