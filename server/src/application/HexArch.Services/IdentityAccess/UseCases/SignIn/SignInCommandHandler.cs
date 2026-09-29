using FluentValidation;
using HexArch.Events.IdentityAccess;
using HexArch.Models.IdentityAccess;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Clock;
using HexArch.Services.IdentityAccess.Ports.Output.Events;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.SignIn
{
    public class SignInCommandHandler : ISignInCommandHandler
    {
        public const int MaxFailedSignInAttempts = 5;
        public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        private const string InvalidCredentials = "Invalid email or password.";

        private readonly IValidator<SignInCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly IJwtTokenGenerator tokenGenerator;
        private readonly ISystemClock clock;
        private readonly IEventDispatcher events;

        public SignInCommandHandler(
            IValidator<SignInCommand> validator,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator,
            ISystemClock clock,
            IEventDispatcher events)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.tokenGenerator = tokenGenerator;
            this.clock = clock;
            this.events = events;
        }

        public async Task<SignInResult> Handle(SignInCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToArray();
                return SignInResult.Failed(errors);
            }

            var user = await userRepository.GetUser(command.Email);
            if (user is null)
            {
                // Deliberately vague so a caller can't use this endpoint to enumerate registered emails.
                return SignInResult.Failed(InvalidCredentials);
            }

            var now = clock.UtcNow;

            // Checked before the password, so a locked account stays locked even for a correct guess.
            if (user.LockedOutUntil is not null && user.LockedOutUntil > now)
            {
                return SignInResult.Failed("This account is temporarily locked. Try again later.");
            }

            if (!passwordHasher.Verify(command.Password, user.Password, user.Salt))
            {
                await RecordFailedAttempt(user, now, cancellationToken);
                return SignInResult.Failed(InvalidCredentials);
            }

            if (!user.Active)
            {
                return SignInResult.Failed("This account is inactive.");
            }

            if (user.FailedSignInCount != 0 || user.LockedOutUntil is not null)
            {
                user.FailedSignInCount = 0;
                user.LockedOutUntil = null;
                await userRepository.UpdateUser(user);
            }

            var token = tokenGenerator.GenerateToken(user);

            return SignInResult.Succeeded(token.Value, token.ExpiresOnUtc);
        }

        private async Task RecordFailedAttempt(User user, DateTime now, CancellationToken cancellationToken)
        {
            user.FailedSignInCount++;

            if (user.FailedSignInCount < MaxFailedSignInAttempts)
            {
                await userRepository.UpdateUser(user);
                return;
            }

            // The counter restarts once the lock is set, so the next failure after the lock expires
            // starts a fresh window rather than locking the account again immediately.
            user.LockedOutUntil = now + LockoutDuration;
            user.FailedSignInCount = 0;

            await userRepository.UpdateUser(user);

            await events.Dispatch(new UserLockedOut(user.Id, user.LockedOutUntil.Value, now), cancellationToken);
        }
    }
}
