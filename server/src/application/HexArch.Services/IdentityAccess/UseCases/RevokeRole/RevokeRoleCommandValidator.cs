using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.RevokeRole;

namespace HexArch.Services.IdentityAccess.UseCases.RevokeRole
{
    public class RevokeRoleCommandValidator : AbstractValidator<RevokeRoleCommand>
    {
        public RevokeRoleCommandValidator()
        {
            RuleFor(command => command.UserId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("UserId is required.");

            RuleFor(command => command.RoleName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Role name is required.");
        }
    }
}
