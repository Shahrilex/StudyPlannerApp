namespace StudyPlanner.Core.Models;

/// <summary>
/// سطح تسلط بر مبحث - لیست بسته‌ی ۴ گزینه‌ای.
/// </summary>
public enum MasteryLevel
{
    Weak,           // ضعیف
    Medium,         // متوسط
    NeedsReview,    // نیاز به مرور
    Mastered        // مسلط
}

public static class MasteryLevelExtensions
{
    public static string ToPersianDisplayName(this MasteryLevel mastery) => mastery switch
    {
        MasteryLevel.Weak => "ضعیف",
        MasteryLevel.Medium => "متوسط",
        MasteryLevel.NeedsReview => "نیاز به مرور",
        MasteryLevel.Mastered => "مسلط",
        _ => mastery.ToString()
    };
}
