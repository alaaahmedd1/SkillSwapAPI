using SkillSwapAPI.Application.Features.Identity;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace SkillSwapAPI.Application.Common.Interfaces.Identity
{
    public interface ITokenProvider
    {
        Task<Result<TokenResponse>> GenerateJwtTokenAsync(AppUserDto user, CancellationToken ct = default);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
