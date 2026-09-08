namespace StudyPlanner.Core.Models;

/// <summary>
/// اولویت مبحث - لیست بسته‌ی ۴ گزینه‌ای.
/// </summary>
public enum PriorityLevel
{
    Low,        // کم
    Medium,     // متوسط
    Important,  // مهم
    Critical    // فوق‌العاده مهم
}

public static class PriorityLevelExtensions
{
    public static string ToPersianDisplayName(this PriorityLevel priority) => priority switch
    {
        PriorityLevel.Low => "کم",
        PriorityLevel.Medium => "متوسط",
        PriorityLevel.Important => "مهم",
        PriorityLevel.Critical => "فوق‌العاده مهم",
        _ => priority.ToString()
    };
}
