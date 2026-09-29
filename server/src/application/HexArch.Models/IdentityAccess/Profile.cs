using System;

namespace HexArch.Models.IdentityAccess
{
    public class Profile
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public string? Bio { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
