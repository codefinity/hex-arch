using System;

namespace HexArch.Models.IdentityAccess
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public IList<Role> Roles { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string MobileNo { get; set; }
        public bool Active { get; set; }
        public DateTime RegisteredOn { get; set; }
        public Profile? Profile { get; set; }

        // Rotated whenever existing sessions must stop working (password change, sign out,
        // deactivation, role revocation). Every issued token carries the stamp it was issued with.
        public Guid SecurityStamp { get; set; }

        public bool EmailVerified { get; set; }
        // An email change waiting for verification; Email only changes once it is verified.
        public string? PendingEmail { get; set; }
        public string? EmailVerificationTokenHash { get; set; }
        public DateTime? EmailVerificationTokenExpiresOn { get; set; }

        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetTokenExpiresOn { get; set; }

        public int FailedSignInCount { get; set; }
        public DateTime? LockedOutUntil { get; set; }

        public string? DeactivationReason { get; set; }
        public DateTime? DeactivatedOn { get; set; }
        public DateTime? ClosedOn { get; set; }

    }
}
