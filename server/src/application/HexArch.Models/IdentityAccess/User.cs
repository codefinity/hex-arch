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

    }
}
