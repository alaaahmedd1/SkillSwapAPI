using System;
using System.Collections.Generic;
using System.Text;

namespace SkillSwapAPI.Application.Common.Interfaces.Notifications
{
    public interface IPushNotificationService
    {
        Task<bool> SendAsync(
            string deviceToken,
            string title,
            string body,
            Dictionary<string, string>? data = null,
            CancellationToken ct = default);
    }

}
