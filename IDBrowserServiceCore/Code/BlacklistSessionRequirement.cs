using Microsoft.AspNetCore.Authorization;

namespace IDBrowserServiceCore.Code
{
    /// <summary>
    /// Blocks blacklisted sessions set by OpenId Backchannel logout.
    /// </summary>
    public class BlacklistSessionRequirement : IAuthorizationRequirement
    {
    }
}
