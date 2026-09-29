using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories;

public class PaymentOrderRepository : BaseRepository<PaymentOrder>, IPaymentOrderRepository
{
    private readonly ApplicationDbContext _context;

    public PaymentOrderRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<PaymentOrder?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default)
    {
        return await _context.PaymentOrders
            .FirstOrDefaultAsync(p => p.ExternalPaymentIntentId == paymentIntentId, ct);
    }
}