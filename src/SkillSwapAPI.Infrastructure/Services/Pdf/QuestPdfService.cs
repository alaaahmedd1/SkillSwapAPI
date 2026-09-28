using QuestPDF.Fluent;
using QuestPDF.Helpers;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.Services.Pdf
{
    public sealed class QuestPdfService : IPdfService
    {
        public byte[] GenerateTimeReceipt(
            Guid transactionId,
            string referenceCode,
            string title,
            string transactionType,
            int amountMinutes,
            int runningBalanceMinutes,
            DateTimeOffset createdAtUtc)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    page.Header()
                        .Text("Skill Swap - Time Receipt")
                        .FontSize(22)
                        .Bold();

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            column.Item()
                                .Text($"Reference Code: {referenceCode}")
                                .Bold();

                            column.Item()
                                .Text($"Transaction ID: {transactionId}");

                            column.Item()
                                .Text($"Title: {title}");

                            column.Item()
                                .Text($"Transaction Type: {transactionType}");

                            column.Item()
                                .Text($"Amount: {amountMinutes} minutes");

                            column.Item()
                                .Text(
                                    $"Running Balance: {runningBalanceMinutes} minutes");

                            column.Item()
                                .Text(
                                    $"Created At: {createdAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Official Skill Swap Time Receipt");
                });
            });

            return document.GeneratePdf();
        }
    }

}
