namespace HexArch.Authentication.Jwt
{
    /// <summary>Claims this adapter both writes into tokens and reads back when validating them.</summary>
    internal static class HexArchClaimTypes
    {
        /// <summary>The user's security stamp at the time the token was issued.</summary>
        public const string SecurityStamp = "sstamp";
    }
}
