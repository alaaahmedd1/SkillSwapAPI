using System;
using System.Collections.Generic;
using System.Text;

namespace SkillSwapAPI.Infrastructure.Services
{
    using Microsoft.AspNetCore.Hosting;
    using SkillSwapAPI.Application.Common.Interfaces.Notifications;

    public class EmailTemplateService : IEmailTempService
    {
        private readonly IWebHostEnvironment _env;

        public EmailTemplateService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GetOtpTemplate(string otp)
        {
            var path = Path.Combine(_env.ContentRootPath, "Templates/Emails/otp-email.html");

            var html = File.ReadAllText(path);

            html = html.Replace("{{OTP}}", otp)
                       .Replace("{{APP_NAME}}", "SkillSwapAPI");

            return html;
        }

        public string GetReceiptTemplate(
            string userName,
            string transactionReference,
            string transactionDate)
        {
            var path = Path.Combine(
                _env.ContentRootPath,
                "Templates/Emails/receipt-email.html");

            var html = File.ReadAllText(path);

            html = html.Replace("{{USER_NAME}}", userName)
                       .Replace("{{TRANSACTION_REFERENCE}}", transactionReference)
                       .Replace("{{TRANSACTION_DATE}}", transactionDate)
                       .Replace("{{APP_NAME}}", "SkillSwapAPI");

            return html;
        }
    }
}
