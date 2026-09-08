namespace StudyPlanner.App.ViewModels;

public class SubjectStat
{
    public string SubjectName { get; init; } = string.Empty;
    public int TotalMinutes { get; init; }
    public int TopicCount { get; init; }

    /// <summary>پهنای نمودار میله‌ای (پیکسل، ۰ تا ۳۰۰) - نسبت به بیشترین مقدار محاسبه‌شده.</summary>
    public double BarWidth { get; init; }
}

public class MasteryStat
{
    public string MasteryName { get; init; } = string.Empty;
    public int Count { get; init; }
    public double BarWidth { get; init; }
}
