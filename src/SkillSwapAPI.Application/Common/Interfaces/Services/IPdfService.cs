using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Common.Interfaces.Services
{
    public interface IPdfService
    {
        byte[] GenerateTimeReceipt(
            Guid transactionId,
            string referenceCode,
            string title,
            string transactionType,
            int amountMinutes,
            int runningBalanceMinutes,
            DateTimeOffset createdAtUtc);
    }
}
