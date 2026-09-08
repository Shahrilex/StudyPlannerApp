using ClosedXML.Excel;
using StudyPlanner.Core.Interfaces;
using StudyPlanner.Core.Models;

namespace StudyPlanner.Services;

/// <summary>
/// خروجی اکسل - فقط «نوشتن». هیچ فایل موجودی خوانده یا ویرایش نمی‌شود؛
/// هر خروجی یک فایل تازه است.
/// </summary>
public class ExcelExportService : IExcelExportService
{
    private const int TopicsStartColumn = 1;  // A
    private const int SessionsStartColumn = 12; // L - فاصله از جدول مباحث (که حالا ۹ ستون شده)

    private readonly IPersianDateService _persianDateService;

    public ExcelExportService(IPersianDateService persianDateService)
    {
        _persianDateService = persianDateService;
    }

    public void ExportSingleDay(StudyDay studyDay, string filePath)
    {
        using var workbook = new XLWorkbook();
        WriteDaySheet(workbook, studyDay);
        workbook.SaveAs(filePath);
    }

    public void ExportAllDays(IEnumerable<StudyDay> studyDays, string filePath)
    {
        using var workbook = new XLWorkbook();
        var usedNames = new HashSet<string>();

        foreach (var day in studyDays.OrderBy(d => d.GregorianDate))
        {
            WriteDaySheet(workbook, day, usedNames);
        }

        if (workbook.Worksheets.Count == 0)
        {
            workbook.Worksheets.Add("خالی");
        }

        workbook.SaveAs(filePath);
    }

    private void WriteDaySheet(XLWorkbook workbook, StudyDay studyDay, HashSet<string>? usedNames = null)
    {
        var jalaliText = string.IsNullOrWhiteSpace(studyDay.JalaliDateText)
            ? _persianDateService.ToJalaliText(studyDay.GregorianDate)
            : studyDay.JalaliDateText;

        var sheetName = SanitizeSheetName(jalaliText.Replace("/", "-"), usedNames);
        var sheet = workbook.Worksheets.Add(sheetName);
        sheet.RightToLeft = true;

        sheet.Cell(2, 2).Value = jalaliText;
        sheet.Cell(2, 2).Style.Font.Bold = true;
        sheet.Cell(2, 2).Style.Font.FontSize = 14;

        WriteTopicsTable(sheet, studyDay.Topics, startRow: 4, startColumn: TopicsStartColumn);
        WriteSessionsTable(sheet, studyDay.Sessions, startRow: 4, startColumn: SessionsStartColumn);

        sheet.Columns().AdjustToContents();
    }

    private static void WriteTopicsTable(IXLWorksheet sheet, List<StudyTopic> topics, int startRow, int startColumn)
    {
        string[] headers = { "ردیف", "عنوان درس", "مبحث مطالعه", "ساعت شروع", "ساعت پایان", "زمان مطالعه (دقیقه)", "اولویت", "سطح تسلط", "توضیحات" };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(startRow, startColumn + i);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F3B57");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var row = startRow + 1;
        foreach (var topic in topics.OrderBy(t => t.RowNumber))
        {
            sheet.Cell(row, startColumn).Value = topic.RowNumber;
            sheet.Cell(row, startColumn + 1).Value = topic.Subject.ToPersianDisplayName();
            sheet.Cell(row, startColumn + 2).Value = topic.Topic;
            sheet.Cell(row, startColumn + 3).Value = topic.StartTime.ToString(@"hh\:mm");
            sheet.Cell(row, startColumn + 4).Value = topic.EndTime.ToString(@"hh\:mm");
            sheet.Cell(row, startColumn + 5).Value = topic.AllocatedMinutes;
            sheet.Cell(row, startColumn + 6).Value = topic.Priority.ToPersianDisplayName();
            sheet.Cell(row, startColumn + 7).Value = topic.Mastery.ToPersianDisplayName();
            sheet.Cell(row, startColumn + 8).Value = topic.Notes;
            row++;
        }
    }

    private static void WriteSessionsTable(IXLWorksheet sheet, List<StudySession> sessions, int startRow, int startColumn)
    {
        string[] headers = { "تاریخ", "زمان شروع", "زمان پایان", "مرور اول", "مرور دوم", "مرور سوم" };

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(startRow, startColumn + i);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#8A6D1F");
            cell.Style.Font.FontColor = XLColor.White;
        }

        var row = startRow + 1;
        foreach (var session in sessions.OrderBy(s => s.Date))
        {
            sheet.Cell(row, startColumn).Value = session.Date;
            sheet.Cell(row, startColumn).Style.DateFormat.Format = "yyyy-mm-dd";
            sheet.Cell(row, startColumn + 1).Value = session.StartTime.ToString(@"hh\:mm");
            sheet.Cell(row, startColumn + 2).Value = session.EndTime.ToString(@"hh\:mm");
            sheet.Cell(row, startColumn + 3).Value = session.ReviewDate1;
            sheet.Cell(row, startColumn + 3).Style.DateFormat.Format = "yyyy-mm-dd";
            sheet.Cell(row, startColumn + 4).Value = session.ReviewDate2;
            sheet.Cell(row, startColumn + 4).Style.DateFormat.Format = "yyyy-mm-dd";
            sheet.Cell(row, startColumn + 5).Value = session.ReviewDate3;
            sheet.Cell(row, startColumn + 5).Style.DateFormat.Format = "yyyy-mm-dd";
            row++;
        }
    }

    private static string SanitizeSheetName(string name, HashSet<string>? usedNames)
    {
        var invalidChars = new[] { '\\', '/', '*', '[', ']', ':', '?' };
        var clean = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());
        if (clean.Length > 31) clean = clean[..31];
        if (string.IsNullOrWhiteSpace(clean)) clean = "روز";

        if (usedNames is null) return clean;

        var candidate = clean;
        var suffix = 1;
        while (!usedNames.Add(candidate))
        {
            var suffixText = $"_{suffix++}";
            candidate = clean[..Math.Min(clean.Length, 31 - suffixText.Length)] + suffixText;
        }

        return candidate;
    }
}
