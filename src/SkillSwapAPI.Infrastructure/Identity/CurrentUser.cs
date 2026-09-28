using Microsoft.AspNetCore.Http;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using System.Security.Claims;

namespace SkillSwapAPI.Infrastructure.Identity
{
      public sealed class CurrentUser : IUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid Id
        {
            get
            {
                var userId = _httpContextAccessor.HttpContext?
                    .User
                    .FindFirstValue(ClaimTypes.NameIdentifier);

                if (!Guid.TryParse(userId, out var id))
                {
                    throw new UnauthorizedAccessException(
                        "Authenticated user ID is missing.");
                }

                return id;
            }
        }
    
    }
}
