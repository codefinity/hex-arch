using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.UpdateProfile;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.UpdateProfile
{
    public class UpdateProfileCommandHandler : IUpdateProfileCommandHandler
    {
        private readonly IValidator<UpdateProfileCommand> validator;
        private readonly ICurrentUserProvider currentUserProvider;
        private readonly IUserRepository userRepository;
        private readonly IProfileRepository profileRepository;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public UpdateProfileCommandHandler(
            IValidator<UpdateProfileCommand> validator,
            ICurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            IProfileRepository profileRepository,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.currentUserProvider = currentUserProvider;
            this.userRepository = userRepository;
            this.profileRepository = profileRepository;
            this.clock = clock;
            this.events = events;
        }

        public async Task<UpdateProfileResult> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {

            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return UpdateProfileResult.Failed(errors);
            }

            var user = await userRepository.GetUser(currentUserProvider.UserId);
            if (user is null)
            {
                return UpdateProfileResult.Failed("User not found.");
            }

            var profile = await profileRepository.GetProfile(currentUserProvider.UserId);
            var isNewProfile = profile is null;
            profile ??= new Profile { UserId = currentUserProvider.UserId };

            profile.Bio = command.Bio;
            profile.Address = command.Address;
            profile.DateOfBirth = command.DateOfBirth;
            profile.AvatarUrl = command.AvatarUrl;
            profile.UpdatedOn = clock.UtcNow;

            if (isNewProfile)
            {
                await profileRepository.AddProfile(profile);
            }
            else
            {
                await profileRepository.UpdateProfile(profile);
            }

            // Raised only after the profile is committed, so any listener projecting a read model
            // always sees durable data.
            await events.Dispatch(new UserProfileUpdated(profile.UserId, clock.UtcNow), cancellationToken);

            return UpdateProfileResult.Succeeded(profile.UserId);
        }
    }
}
