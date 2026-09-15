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
    private SubjectType _subject;
    public SubjectType Subject
    {
        get => _subject;
        set
        {
            if (_subject == value) return;
            _subject = value;
            // انتخاب‌ها فقط در چارچوب همان درس معنا دارند.
            Topic = string.Empty;
            ArticleNumbers = string.Empty;
            SelectedTopicKeys = string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasTopicCatalog));
        }
    }

    /// <summary>برای این درس، فهرست مبحث در داده‌های مرجع موجود است.</summary>
    public bool HasTopicCatalog => CivilTopicCatalog.GetOptions(Subject).Count > 0;

    /// <summary>مبحث مطالعه. برای درس مدنی از فهرست چندانتخابی پر می‌شود.</summary>
    private string _topic = string.Empty;
    public string Topic
    {
        get => _topic;
        set
        {
            if (_topic == value) return;
            _topic = value;
            OnPropertyChanged();
        }
    }

    /// <summary>شماره مواد متناظر با مباحث انتخاب‌شده.</summary>
    private string _articleNumbers = string.Empty;
    public string ArticleNumbers
    {
        get => _articleNumbers;
        set
        {
            if (_articleNumbers == value) return;
            _articleNumbers = value;
            OnPropertyChanged();
        }
    }

    /// <summary>کلیدهای داخلی انتخاب‌ها؛ برای بازیابی انتخاب‌های تکراریِ هم‌نام استفاده می‌شود.</summary>
    private string _selectedTopicKeys = string.Empty;
    public string SelectedTopicKeys
    {
        get => _selectedTopicKeys;
        set
        {
            if (_selectedTopicKeys == value) return;
            _selectedTopicKeys = value;
            OnPropertyChanged();
        }
    }

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
