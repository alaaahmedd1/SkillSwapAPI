using System;
using System.Collections.Generic;
using System.Text;

namespace SkillSwapAPI.Application.Common.Settings
{
    public class OtpSettings
    {
        public const string SectionName = "OtpSettings";
        public int OtpLength { get; set; } = 6;
        public int OtpExpiryMinutes { get; set; } = 5;
        public int MaxResendAttempts { get; set; } = 3;
        public bool RequireEmailConfirmation { get; set; } = true;
    }
}
