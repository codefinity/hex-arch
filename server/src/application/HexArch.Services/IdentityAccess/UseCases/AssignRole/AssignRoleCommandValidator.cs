using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.AssignRole;

namespace HexArch.Services.IdentityAccess.UseCases.AssignRole
{
    public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
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
