using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;

namespace HexArch.Services.IdentityAccess.UseCases.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {

            RuleFor(command => command.Bio)
                .MaximumLength(500)
                .WithMessage("Bio must not exceed 500 characters.");

            RuleFor(command => command.Address)
                .MaximumLength(300)
                .WithMessage("Address must not exceed 300 characters.");

            RuleFor(command => command.AvatarUrl)
                .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
                .When(command => !string.IsNullOrEmpty(command.AvatarUrl))
                .WithMessage("Avatar URL must be a valid absolute URL.");

            RuleFor(command => command.DateOfBirth)
                .LessThan(_ => DateTime.UtcNow)
                .When(command => command.DateOfBirth.HasValue)
                .WithMessage("Date of birth must be in the past.");
        }
    }
}
