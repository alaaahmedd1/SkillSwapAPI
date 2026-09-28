using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Common.Interfaces.Services
{

    public interface ITimeLedgerService
    {
        Task SettleAsync(
            Guid learnerId,
            Guid teacherId,
            int durationMinutes,
            Guid? swapRequestId,
            CancellationToken cancellationToken = default);
    }

}
