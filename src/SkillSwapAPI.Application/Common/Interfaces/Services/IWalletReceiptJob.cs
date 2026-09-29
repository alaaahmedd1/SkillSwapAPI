using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Common.Interfaces.Services
{
    public interface IWalletReceiptJob
    {
    Task SendReceiptAsync(
    Guid userId,
    Guid transactionId,
    CancellationToken cancellationToken = default);
    }
}
