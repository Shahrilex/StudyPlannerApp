namespace StudyPlanner.Core.Models;

/// <summary>
/// یک روز مطالعاتی - ظرف اصلی نگه‌دارنده‌ی مباحث و جلسات آن روز.
/// </summary>
public class StudyDay
{
    public int Id { get; set; }

    /// <summary>کلید طبیعی و یکتا - تاریخ میلادی همان روز.</summary>
    public DateTime GregorianDate { get; set; }

    /// <summary>متن تاریخ شمسی برای نمایش (مثلاً «۱۴۰۴/۰۵/۰۴»).</summary>
    public string JalaliDateText { get; set; } = string.Empty;

    /// <summary>عنوان اختیاری روز.</summary>
    public string? Title { get; set; }

    public List<StudyTopic> Topics { get; set; } = new();

    public List<StudySession> Sessions { get; set; } = new();
}
