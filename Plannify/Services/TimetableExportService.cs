using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Plannify.Models;

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
                row.RelativeColumn(1).Text($"Semester: {semester}").FontSize(10);
                row.RelativeColumn(1).Text($"Academic Year: {academicYear}").FontSize(10);
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
                row.RelativeColumn(1).Text($"Generated on {DateTime.Now:dd MMM yyyy HH:mm:ss}").FontSize(9).Italic();
                row.RelativeColumn(1).AlignRight().Text($"Page 1").FontSize(9);
            });
        });
    }

    /// <summary>
    /// Exports class timetable as Excel workbook
    /// </summary>
    public byte[] ExportClassTimetableExcel(
        string batchName,
        string semesterName,
        Dictionary<string, Dictionary<string, TimetableSlot?>> grid,
        List<string> timeRanges,
        List<string> days)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(batchName);

        // Freeze header row and first column
        worksheet.SheetView.FreezeRows(2);
        worksheet.SheetView.FreezeColumns(1);

        // Header: Semester and batch info
        var headerCell = worksheet.Cell(1, 1);
        headerCell.Value = $"Timetable - {batchName} ({semesterName})";
        headerCell.Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Range("A1:G1").Merge();

        // Column headers
        worksheet.Cell(2, 1).Value = "Time";
        worksheet.Cell(2, 1).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1);
        worksheet.Cell(2, 1).Style.Font.Bold = true;

        for (int dayIdx = 0; dayIdx < days.Count; dayIdx++)
        {
            var cell = worksheet.Cell(2, dayIdx + 2);
            cell.Value = days[dayIdx];
            cell.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1);
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // Data rows
        for (int rowIdx = 0; rowIdx < timeRanges.Count; rowIdx++)
        {
            var timeRange = timeRanges[rowIdx];
            var excelRow = rowIdx + 3;

            worksheet.Cell(excelRow, 1).Value = timeRange;
            worksheet.Cell(excelRow, 1).Style.Font.Bold = true;

            for (int dayIdx = 0; dayIdx < days.Count; dayIdx++)
            {
                var day = days[dayIdx];
                var slot = grid[day][timeRange];
                var cell = worksheet.Cell(excelRow, dayIdx + 2);

                if (slot != null)
                {
                    cell.Value = $"{slot.Subject?.Name}\n{slot.Teacher?.Initials}\n{slot.Room?.RoomNumber}";
                    cell.Style.Alignment.WrapText = true;

                    // Apply color based on slot type
                    var bgColor = slot.SlotType switch
                    {
                        "Lab" => XLColor.FromArgb(207, 226, 255),
                        "GAP" => XLColor.FromArgb(233, 236, 239),
                        "Elective" => XLColor.FromArgb(229, 213, 240),
                        _ => XLColor.White
                    };

                    cell.Style.Fill.BackgroundColor = bgColor;
                }
                else
                {
                    cell.Value = "Free";
                    cell.Style.Fill.BackgroundColor = XLColor.FromArgb(240, 240, 240);
                }

                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Exports conflict report as Excel workbook with three sheets
    /// </summary>
    public byte[] ExportConflictReportExcel(List<ConflictReport> conflicts)
    {
        var workbook = new XLWorkbook();

        var teacherConflicts = conflicts.Where(c => c.ConflictType == "TeacherConflict").ToList();
        var roomConflicts = conflicts.Where(c => c.ConflictType == "RoomConflict").ToList();
        var classConflicts = conflicts.Where(c => c.ConflictType == "ClassConflict").ToList();

        if (teacherConflicts.Count > 0)
            CreateConflictSheet(workbook, "Teacher Conflicts", teacherConflicts, "Teacher");

        if (roomConflicts.Count > 0)
            CreateConflictSheet(workbook, "Room Conflicts", roomConflicts, "Room");

        if (classConflicts.Count > 0)
            CreateConflictSheet(workbook, "Class Conflicts", classConflicts, "Class");

        if (workbook.Worksheets.Count == 0)
        {
            // Create empty info sheet
            var ws = workbook.Worksheets.Add("Info");
            ws.Cell(1, 1).Value = "No conflicts detected";
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void CreateConflictSheet(XLWorkbook workbook, string sheetName, List<ConflictReport> conflicts, string affectedEntityLabel)
    {
        var worksheet = workbook.Worksheets.Add(sheetName);
        worksheet.SheetView.FreezeRows(1);

        // Headers
        var headerRow = worksheet.Row(1);
        headerRow.Cell(1).Value = affectedEntityLabel;
        headerRow.Cell(2).Value = "Day";
        headerRow.Cell(3).Value = "Time Range";
        headerRow.Cell(4).Value = "Slot 1";
        headerRow.Cell(5).Value = "Slot 2";

        foreach (var cell in headerRow.Cells())
        {
            cell.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1);
            cell.Style.Font.Bold = true;
        }

        // Data rows
        int rowNum = 2;
        foreach (var conflict in conflicts)
        {
            worksheet.Cell(rowNum, 1).Value = conflict.AffectedEntity;
            worksheet.Cell(rowNum, 2).Value = conflict.Day;
            worksheet.Cell(rowNum, 3).Value = $"{conflict.StartTime:HH:mm}-{conflict.EndTime:HH:mm}";
            worksheet.Cell(rowNum, 4).Value = conflict.Slot1Description;
            worksheet.Cell(rowNum, 5).Value = conflict.Slot2Description;

            rowNum++;
        }

        worksheet.Columns().AdjustToContents();
    }
}
