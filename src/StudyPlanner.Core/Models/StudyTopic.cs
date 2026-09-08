using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyPlanner.Core.Models;

/// <summary>
/// یک ردیف از جدول «برنامه ریز مطالعاتی».
/// طبق تصمیم نهایی: ساعت شروع/پایان اینجا وارد می‌شود و «زمان مطالعه» (دقیقه)
/// خودش از تفاضل این دو محاسبه می‌شود - دیگر لیست بسته‌ی ۶۰/۹۰/... نیست.
/// </summary>
public class StudyTopic : INotifyPropertyChanged
{
    public int Id { get; set; }

    /// <summary>ردیف - خودکار شماره‌گذاری می‌شود (نه ستون تصمیم‌گیری کاربر).</summary>
    public int RowNumber { get; set; }

    /// <summary>عنوان درس - لیست بسته‌ی ۹ گزینه‌ای.</summary>
    public SubjectType Subject { get; set; }

    /// <summary>مبحث مطالعه - متن آزاد، دستی.</summary>
    public string Topic { get; set; } = string.Empty;

    private TimeSpan _startTime = new(9, 0, 0);

    /// <summary>ساعت شروع مطالعه‌ی این مبحث.</summary>
    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime == value) return;
            _startTime = value;
            OnPropertyChanged();
            RecomputeAllocatedMinutes();
        }
    }

    private TimeSpan _endTime = new(10, 0, 0);

    /// <summary>ساعت پایان مطالعه‌ی این مبحث.</summary>
    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime == value) return;
            _endTime = value;
            OnPropertyChanged();
            RecomputeAllocatedMinutes();
        }
    }

    private int _allocatedMinutes;

    /// <summary>
    /// زمان مطالعه (دقیقه) - فقط‌خواندنی، خودکار از تفاضل EndTime - StartTime محاسبه می‌شود.
    /// اگر پایان قبل از شروع باشد صفر در نظر گرفته می‌شود.
    /// </summary>
    public int AllocatedMinutes
    {
        get => _allocatedMinutes;
        private set
        {
            if (_allocatedMinutes == value) return;
            _allocatedMinutes = value;
            OnPropertyChanged();
        }
    }

    /// <summary>اولویت - لیست بسته‌ی ۴ گزینه‌ای.</summary>
    public PriorityLevel Priority { get; set; }

    /// <summary>سطح تسلط - لیست بسته‌ی ۴ گزینه‌ای.</summary>
    public MasteryLevel Mastery { get; set; }

    /// <summary>توضیحات - متن آزاد.</summary>
    public string Notes { get; set; } = string.Empty;

    // Foreign key
    public int StudyDayId { get; set; }
    public StudyDay? StudyDay { get; set; }

    private void RecomputeAllocatedMinutes()
    {
        var diff = (EndTime - StartTime).TotalMinutes;
        AllocatedMinutes = diff > 0 ? (int)Math.Round(diff) : 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
