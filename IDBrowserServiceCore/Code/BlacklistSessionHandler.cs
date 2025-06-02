using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;

namespace IDBrowserServiceCore.Code
{
    /// <summary>
    /// Blocks blacklisted sessions set by OpenId Backchannel logout.
    /// </summary>
    public class BlacklistSessionHandler : AuthorizationHandler<BlacklistSessionRequirement>
    {
        private readonly IFusionCache _cache;

        public BlacklistSessionHandler(IFusionCache cache)
        {
            _cache = cache;
        }

        protected override async Task<Task> HandleRequirementAsync(
            AuthorizationHandlerContext context,
            BlacklistSessionRequirement requirement)
        {
            var sessionId = context.User.FindFirst("sid")?.Value;

            if (string.IsNullOrEmpty(sessionId))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var isBlacklisted = await _cache.GetOrDefaultAsync($"session_blacklist:{sessionId}", false);

            if (isBlacklisted)
            {
                context.Fail();
            }
            else
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
