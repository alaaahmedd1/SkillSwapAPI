using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Payments;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Payments.Dtos;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Domain.Modules.Payments.Enums;

namespace SkillSwapAPI.Application.Features.Payments.Commands.InitiateCheckout;

public sealed class InitiateCheckoutCommandHandler(
    IUnitOfWork unitOfWork,
    IPaymentGatewayService paymentGatewayService)
    : IRequestHandler<InitiateCheckoutCommand, CheckoutResponseDto>
{
    public async Task<CheckoutResponseDto> Handle(InitiateCheckoutCommand command, CancellationToken ct)
    {
        var userId = command.UserId;

        var package = await unitOfWork.CreditPackages.GetByIdAsync(command.PackageId);
        if (package is null || !package.IsActive)
        {
            throw new KeyNotFoundException("Credit package not found or inactive.");
        }

        (string paymentIntentId, string clientSecret) = await paymentGatewayService.CreatePaymentIntentAsync(
            package.Price,
            package.Currency,
            userId,
            package.Id,
            ct);

        var order = new PaymentOrder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreditPackageId = package.Id,
            Amount = package.Price,
            Currency = package.Currency,
            Status = PaymentStatus.Pending,
            ExternalPaymentIntentId = paymentIntentId,
            ClientSecret = clientSecret,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await unitOfWork.PaymentOrders.AddAsync(order, ct);
        await unitOfWork.CompleteAsync(ct);

        return new CheckoutResponseDto(
            order.Id,
            clientSecret,
            order.Amount,
            order.Currency);
    }
}