using System;
using System.Collections.Generic;
using System.Text;

namespace SkillSwapAPI.Application.Common.Settings
{
    public sealed class OtpSettings
    {
        public const string SectionName = "OtpSettings";

        public int OtpLength { get; init; }
        public int OtpExpiryMinutes { get; init; }
        public int MaxResendAttempts { get; init; }
        public bool RequireEmailConfirmation { get; init; }
    }
}
