using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories;

public class PaymentOrderRepository : BaseRepository<PaymentOrder>, IPaymentOrderRepository
{
    public PaymentOrderRepository(ApplicationDbContext context) : base(context)
    {
    }
}