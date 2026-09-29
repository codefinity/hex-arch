using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.ApplyForSellerAccount;

namespace HexArch.Services.IdentityAccess.UseCases.ApplyForSellerAccount
{
    public class ApplyForSellerAccountCommandValidator : AbstractValidator<ApplyForSellerAccountCommand>
    {
        public ApplyForSellerAccountCommandValidator()
        {
            RuleFor(command => command.BusinessName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Business name is required.")
                .MaximumLength(200)
                .WithMessage("Business name must not exceed 200 characters.");
        }
    }
}
