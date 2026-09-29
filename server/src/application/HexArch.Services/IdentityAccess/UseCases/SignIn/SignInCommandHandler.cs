using FluentValidation;
using HexArch.Services.IdentityAccess.Ports.Input.Commands.SignIn;
using HexArch.Services.IdentityAccess.Ports.Output.Authentication;
using HexArch.Services.IdentityAccess.Ports.Output.Persistance;

namespace HexArch.Services.IdentityAccess.UseCases.SignIn
{
    public class SignInCommandHandler : ISignInCommandHandler
    {
        private readonly IValidator<SignInCommand> validator;
        private readonly IUserRepository userRepository;
        private readonly IPasswordHasher passwordHasher;
        private readonly IJwtTokenGenerator tokenGenerator;

        public SignInCommandHandler(
            IValidator<SignInCommand> validator,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator)
        {
            this.validator = validator;
            this.userRepository = userRepository;
            this.passwordHasher = passwordHasher;
            this.tokenGenerator = tokenGenerator;
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
            if (user is null || !passwordHasher.Verify(command.Password, user.Password, user.Salt))
            {
                // Deliberately vague so a caller can't use this endpoint to enumerate registered emails.
                return SignInResult.Failed("Invalid email or password.");
            }

            if (!user.Active)
            {
                return SignInResult.Failed("This account is inactive.");
            }

            var token = tokenGenerator.GenerateToken(user);

            return SignInResult.Succeeded(token.Value, token.ExpiresOnUtc);
        }
    }
}
