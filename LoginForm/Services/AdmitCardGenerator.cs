using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;

namespace LoginForm.Services
{
    public class AdmitCardGenerator
    {
        public static byte[] GeneratePdf(string studentEmail, DateOnly examDate, int examId)
        {
            QuestPDF.Settings.License = LicenseType.Community; // Required for free version

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // --- Header ---
                    page.Header()
                        .Text("OFFICIAL ADMIT CARD")
                        .SemiBold().FontSize(24).FontColor(Colors.Green.Medium);

                    // --- Content ---
                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            x.Item().Text($"Exam Ticket ID: #{examId}").FontSize(10).FontColor(Colors.Grey.Medium);

                            x.Item().LineHorizontal(1).LineColor(Colors.Green.Lighten1);

                            x.Item().Text($"Candidate Email: {studentEmail}").Bold().FontSize(16);
                            x.Item().Text($"Examination Date: {examDate:dd MMMM yyyy}").FontSize(14);

                            x.Item().PaddingTop(20).Text("Instructions:").Bold();
                            x.Item().Text("1. Please bring a valid ID proof.");
                            x.Item().Text("2. Login to the portal 15 minutes before the exam time.");
                            x.Item().Text("3. Do not refresh the page during the exam.");

                            x.Item().PaddingTop(50).AlignRight().Text("Controller of Examinations").Italic();
                        });

                    // --- Footer ---
                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}