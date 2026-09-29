using System;

namespace HexArch.Models.IdentityAccess
{
    public class SellerApplication
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public SellerApplicationStatus Status { get; set; }
        public DateTime SubmittedOn { get; set; }
        public DateTime? DecidedOn { get; set; }
        public Guid? DecidedBy { get; set; }
        public string? DecisionNote { get; set; }
    }
}
