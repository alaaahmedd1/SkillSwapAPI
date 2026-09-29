using SkillSwapAPI.Domain.Modules.Payments.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos;

public interface IPaymentOrderRepository : IBaseRepository<PaymentOrder>
{
    Task<PaymentOrder?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default);
}