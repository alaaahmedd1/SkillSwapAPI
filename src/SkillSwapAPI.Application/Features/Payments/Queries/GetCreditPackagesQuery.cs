using MediatR;
using SkillSwapAPI.Application.Features.Payments.Dtos;

namespace SkillSwapAPI.Application.Features.Payments.Queries.GetCreditPackages;

public sealed record GetCreditPackagesQuery : IRequest<IReadOnlyList<CreditPackageDto>>;