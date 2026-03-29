using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Plannify.Domain.Entities;

namespace Plannify.Services;

public class TimetableExportService
{
    public TimetableExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>
    /// Exports class timetable as PDF
    /// </summary>
    public byte[] ExportClassTimetablePdf(
        string batchName, 
        string semesterName, 
        string academicYear,
        Dictionary<string, Dictionary<string, TimetableSlot?>> grid,
        List<string> timeRanges,
        List<string> days)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(842, 595);  // A4 Landscape in mm
                page.Margin(20);

                page.Header().Element(header => BuildPdfHeader(header, batchName, semesterName, academicYear));
                page.Content().Element(content => BuildTimetableContent(content, grid, timeRanges, days));
                page.Footer().Element(footer => BuildPdfFooter(footer));
            });
        });

        return document.GeneratePdf();
    }

    /// <summary>
    /// Exports teacher timetable as PDF
    /// </summary>
    public byte[] ExportTeacherTimetablePdf(
        string teacherName,
        string semesterName,
        string academicYear,
        Dictionary<string, Dictionary<string, TimetableSlot?>> grid,
        List<string> timeRanges,
        List<string> days)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(842, 595);  // A4 Landscape in mm
                page.Margin(20);

                page.Header().Element(header => BuildPdfHeader(header, $"Teacher: {teacherName}", semesterName, academicYear));
                page.Content().Element(content => BuildTeacherTimetableContent(content, grid, timeRanges, days));
                page.Footer().Element(footer => BuildPdfFooter(footer));
            });
        });

        return document.GeneratePdf();
    }

    private static void BuildPdfHeader(IContainer container, string title, string semester, string academicYear)
    {
        container.Column(column =>
        {
            column.Item().Text("Faculty TimeGrid Pro").Bold().FontSize(14);
            column.Item().Text(title).Bold().FontSize(12);
            column.Item().Row(row =>
            {
                row.RelativeItem(1).Text($"Semester: {semester}").FontSize(10);
                row.RelativeItem(1).Text($"Academic Year: {academicYear}").FontSize(10);
            });
            column.Item().PaddingBottom(10).LineHorizontal(1);
        });
    }

    private static void BuildTimetableContent(IContainer container, Dictionary<string, Dictionary<string, TimetableSlot?>> grid, List<string> timeRanges, List<string> days)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    foreach (var day in days)
                        columns.RelativeColumn(1);
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Element(c => c.Padding(5).Background("#0d6efd").Text("Time").FontColor("#ffffff").Bold());
                    foreach (var day in days)
                        header.Cell().Element(c => c.Padding(5).Background("#0d6efd").Text(day.Substring(0, 3)).FontColor("#ffffff").Bold());
                });

                // Data rows
                foreach (var timeRange in timeRanges)
                {
                    table.Cell().Element(c => c.Padding(5).Border(1).BorderColor("#dee2e6").Text(timeRange).FontSize(9));

                    foreach (var day in days)
                    {
                        var slot = grid[day][timeRange];
                        if (slot != null)
                        {
                            var bgColor = slot.SlotType switch
                            {
                                "Lab" => "#cfe2ff",
                                "GAP" => "#e9ecef",
                                "Elective" => "#e5d5f0",
                                _ => "#ffffff"
                            };
                            table.Cell().Element(c => c.Padding(5).Border(1).BorderColor("#dee2e6").Background(bgColor).Text($"{slot.Subject?.Code}\n{slot.Teacher?.Initials}").FontSize(8));
                        }
                        else
                        {
                            table.Cell().Element(c => c.Padding(5).Border(1).BorderColor("#dee2e6").Text("Free").FontSize(8).Light());
                        }
                    }
                }
            });
        });
    }

    private static void BuildTeacherTimetableContent(IContainer container, Dictionary<string, Dictionary<string, TimetableSlot?>> grid, List<string> timeRanges, List<string> days)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);
                    foreach (var day in days)
                        columns.RelativeColumn(1);
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Element(c => c.Padding(5).Background("#0d6efd").Text("Time").FontColor("#ffffff").Bold());
                    foreach (var day in days)
                        header.Cell().Element(c => c.Padding(5).Background("#0d6efd").Text(day.Substring(0, 3)).FontColor("#ffffff").Bold());
                });

                // Data rows
                foreach (var timeRange in timeRanges)
                {
                    table.Cell().Element(c => c.Padding(5).Border(1).BorderColor("#dee2e6").Text(timeRange).FontSize(9));

                    foreach (var day in days)
                    {
                        var slot = grid[day][timeRange];
                        if (slot != null)
                        {
                            var bgColor = slot.SlotType switch
                            {
                                "Lab" => "#cfe2ff",
                                "GAP" => "#e9ecef",
                                "Elective" => "#e5d5f0",
                                _ => "#ffffff"
                            };
                            table.Cell().Element(c => c.Padding(5).Border(1).BorderColor("#dee2e6").Background(bgColor).Text($"{slot.Subject?.Code}\n{slot.ClassBatch?.BatchName}").FontSize(8));
                        }
                        else
                        {
                            table.Cell().Element(c => c.Padding(5).Border(1).BorderColor("#dee2e6").Text("Free").FontSize(8).Light());
                        }
                    }
                }
            });
        });
    }

    private static void BuildPdfFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).LineHorizontal(1);
            column.Item().Row(row =>
            {
                row.RelativeItem(1).Text($"Generated on {DateTime.Now:dd MMM yyyy HH:mm:ss}").FontSize(9).Italic();
                row.RelativeItem(1).AlignRight().Text($"Page 1").FontSize(9);
            });
        });
    }
}
