using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Payments.Dtos;

namespace SkillSwapAPI.Application.Features.Payments.Queries.GetCreditPackages;

public sealed class GetCreditPackagesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetCreditPackagesQuery, IReadOnlyList<CreditPackageDto>>
{
    public async Task<IReadOnlyList<CreditPackageDto>> Handle(GetCreditPackagesQuery request, CancellationToken ct)
    {
        var packages = await unitOfWork.CreditPackages.GetActivePackagesAsync(ct);

        return packages.Select(p => new CreditPackageDto(
            p.Id,
            p.Name,
            p.Description,
            p.CreditsCount,
            p.Price,
            p.Currency)).ToList();
    }
}