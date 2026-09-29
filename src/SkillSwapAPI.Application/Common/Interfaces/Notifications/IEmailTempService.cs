using System;
using System.Collections.Generic;
using System.Text;

namespace SkillSwapAPI.Application.Common.Interfaces.Notifications
{
    public interface IEmailTempService
    {
        string GetOtpTemplate(string otp);

        string GetReceiptTemplate(string userName, string transactionReference, string transactionDate);
    }
}