using StudyPlanner.Core.Models;

namespace StudyPlanner.Core.Interfaces;

public interface IExcelExportService
{
    /// <summary>خروجی یک روز خاص در یک شیت، به فایل مشخص‌شده (همیشه فایل تازه، بدون خواندن فایل موجود).</summary>
    void ExportSingleDay(StudyDay studyDay, string filePath);

    /// <summary>خروجی همه‌ی روزها - هر روز = یک شیت، در یک فایل.</summary>
    void ExportAllDays(IEnumerable<StudyDay> studyDays, string filePath);
}
