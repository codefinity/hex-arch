using System.Security.Claims;
using HexArch.Services.IdentityAccess.Ports.Output.Authorization;
using Microsoft.AspNetCore.Http;

namespace HexArch.Authorization;

public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("No authenticated user is available on the current request.");

            return Guid.Parse(claim);
        }
    }
}
