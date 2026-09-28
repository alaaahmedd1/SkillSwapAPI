using MediatR;
using SkillSwapAPI.Application.Features.Payments.Dtos;

namespace SkillSwapAPI.Application.Features.Payments.Commands.InitiateCheckout;

public sealed record InitiateCheckoutCommand(Guid PackageId) : IRequest<CheckoutResponseDto>;
