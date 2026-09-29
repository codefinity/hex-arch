using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateAccountDetails;

namespace HexArch.Services.IdentityAccess.UseCases.UpdateAccountDetails
{
    public class UpdateAccountDetailsCommandValidator : AbstractValidator<UpdateAccountDetailsCommand>
    {
        public UpdateAccountDetailsCommandValidator()
        {
            RuleFor(command => command.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Name is required.")
                .MaximumLength(200)
                .WithMessage("Name must not exceed 200 characters.");

            RuleFor(command => command.MobileNo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Mobile number is required.")
                .MaximumLength(20)
                .WithMessage("Mobile number must not exceed 20 characters.");
        }
    }
}
