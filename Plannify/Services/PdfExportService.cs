using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Plannify.Domain.Entities;

namespace Plannify.Services;

/// <summary>
/// Generates PDF exports for class and teacher timetables using QuestPDF
/// </summary>
public class PdfExportService
{
    /// <summary>
    /// Generates class timetable PDF
    /// </summary>
    public byte[] GenerateClassTimetablePdf(
        string batchName,
        string semesterLabel,
        List<string> days,
        List<string> timeRanges,
        Dictionary<string, Dictionary<string, TimetableSlot?>> grid)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(842, 595);  // A4 Landscape in points
                page.Margin(20);

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Text($"Timetable — {batchName}").FontSize(16).Bold();
                    col.Item().Text($"Semester: {semesterLabel}").FontSize(11);
                    col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy}").FontSize(9).FontColor("#888");
                    col.Item().PaddingTop(10);

                    // Grid table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            foreach (var _ in timeRanges) { columns.RelativeColumn(1); }
                        });

                        table.Header(header =>
                        {
                            header.Cell().Padding(5).Text("Day / Time").Bold().FontSize(10);
                            foreach (var timeRange in timeRanges)
                            {
                                header.Cell().Padding(5).Text(timeRange).Bold().FontSize(9).AlignCenter();
                            }
                        });

                        foreach (var day in days)
                        {
                            table.Cell().Padding(5).Text(day).Bold().FontSize(10);
                            foreach (var timeRange in timeRanges)
                            {
                                var slot = grid[day][timeRange];
                                if (slot == null)
                                {
                                    table.Cell().Padding(5).Background("#f5f5f5").Text("—").FontSize(8).AlignCenter();
                                }
                                else if (slot.SlotType == "GAP")
                                {
                                    table.Cell().Padding(5).Background("#f0f0f0").Text("GAP").FontSize(8).FontColor("#999").Italic().AlignCenter();
                                }
                                else if (slot.SlotType == "Lab")
                                {
                                    table.Cell().Padding(5).Background("#e8f4f8").Column(c =>
                                    {
                                        c.Item().Text($"{slot.Subject?.Code}").Bold().FontSize(9);
                                        c.Item().Text("[LAB]").FontSize(7).FontColor("#0066cc");
                                        c.Item().Text($"{slot.Teacher?.Initials}").FontSize(8).FontColor("#666");
                                        c.Item().Text($"{slot.Room?.RoomNumber}").FontSize(8).FontColor("#666");
                                    });
                                }
                                else
                                {
                                    table.Cell().Padding(5).Background("#ffffff").Column(c =>
                                    {
                                        c.Item().Text($"{slot.Subject?.Code}").Bold().FontSize(9);
                                        c.Item().Text($"{slot.Teacher?.Initials}").FontSize(8).FontColor("#666");
                                        c.Item().Text($"{slot.Room?.RoomNumber}").FontSize(8).FontColor("#666");
                                    });
                                }
                            }
                        }
                    });

                    col.Item().PaddingTop(10).Text("Faculty TimeGrid — Auto-generated timetable").FontSize(8).FontColor("#aaa");
                });
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Generates teacher timetable PDF
    /// </summary>
    public byte[] GenerateTeacherTimetablePdf(
        string teacherName,
        string semesterLabel,
        List<string> days,
        List<string> timeRanges,
        Dictionary<string, Dictionary<string, TimetableSlot?>> grid)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(842, 595);  // A4 Landscape in points
                page.Margin(20);

                page.Content().Column(col =>
                {
                    // Header
                    col.Item().Text($"Teacher Timetable — {teacherName}").FontSize(16).Bold();
                    col.Item().Text($"Semester: {semesterLabel}").FontSize(11);
                    col.Item().Text($"Generated: {DateTime.Now:dd MMM yyyy}").FontSize(9).FontColor("#888");
                    col.Item().PaddingTop(10);

                    // Grid table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(80);
                            foreach (var _ in timeRanges) { columns.RelativeColumn(1); }
                        });

                        table.Header(header =>
                        {
                            header.Cell().Padding(5).Text("Day / Time").Bold().FontSize(10);
                            foreach (var timeRange in timeRanges)
                            {
                                header.Cell().Padding(5).Text(timeRange).Bold().FontSize(9).AlignCenter();
                            }
                        });

                        foreach (var day in days)
                        {
                            table.Cell().Padding(5).Text(day).Bold().FontSize(10);
                            foreach (var timeRange in timeRanges)
                            {
                                var slot = grid[day][timeRange];
                                if (slot == null)
                                {
                                    table.Cell().Padding(5).Background("#f5f5f5").Text("—").FontSize(8).AlignCenter();
                                }
                                else if (slot.SlotType == "GAP")
                                {
                                    table.Cell().Padding(5).Background("#f0f0f0").Text("GAP").FontSize(8).FontColor("#999").Italic().AlignCenter();
                                }
                                else if (slot.SlotType == "Lab")
                                {
                                    table.Cell().Padding(5).Background("#e8f4f8").Column(c =>
                                    {
                                        c.Item().Text($"{slot.Subject?.Code}").Bold().FontSize(9);
                                        c.Item().Text("[LAB]").FontSize(7).FontColor("#0066cc");
                                        c.Item().Text($"{slot.ClassBatch?.BatchName}").FontSize(8).FontColor("#666");
                                        c.Item().Text($"{slot.Room?.RoomNumber}").FontSize(8).FontColor("#666");
                                    });
                                }
                                else
                                {
                                    table.Cell().Padding(5).Background("#ffffff").Column(c =>
                                    {
                                        c.Item().Text($"{slot.Subject?.Code}").Bold().FontSize(9);
                                        c.Item().Text($"{slot.ClassBatch?.BatchName}").FontSize(8).FontColor("#666");
                                        c.Item().Text($"{slot.Room?.RoomNumber}").FontSize(8).FontColor("#666");
                                    });
                                }
                            }
                        }
                    });

                    col.Item().PaddingTop(10).Text("Faculty TimeGrid — Auto-generated timetable").FontSize(8).FontColor("#aaa");
                });
            });
        });

        return document.GeneratePdf();
    }
}
