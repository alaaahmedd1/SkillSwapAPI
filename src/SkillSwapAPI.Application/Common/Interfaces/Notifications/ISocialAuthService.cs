using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkillSwapAPI.Application.Common.Interfaces.Notifications
{
    public interface ISocialAuthService
    {
        Task<Result<SocialUserInfo>> VerifyTokenAsync(
            string idToken,
            SocialProvider provider,
            CancellationToken ct = default);
    }
}
