using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StudyPlanner.Core.Models;

/// <summary>
/// یک ردیف از جدول «جلسات مطالعاتی» - دقیقاً همین ۶ فیلد.
/// ReviewDate1/2/3 محاسبه‌شده هستند و در دیتابیس ذخیره نمی‌شوند (Ignore در OnModelCreating).
/// INotifyPropertyChanged پیاده‌سازی شده تا با تغییر Date، هر سه ستون مرور بلافاصله
/// در DataGrid به‌روز شوند، بدون نیاز به ذخیره/رفرش.
/// </summary>
public class StudySession : INotifyPropertyChanged
{
    public int Id { get; set; }

    private DateTime _date;

    /// <summary>تاریخ - از تقویم شمسی انتخاب می‌شود.</summary>
    public DateTime Date
    {
        get => _date;
        set
        {
            if (_date == value) return;
            _date = value;
            OnPropertyChanged();
            // با تغییر تاریخ، هر سه تاریخ مرور هم عوض می‌شوند - UI باید مطلع شود.
            OnPropertyChanged(nameof(ReviewDate1));
            OnPropertyChanged(nameof(ReviewDate2));
            OnPropertyChanged(nameof(ReviewDate3));
        }
    }

    private TimeSpan _startTime;

    /// <summary>زمان شروع.</summary>
    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime == value) return;
            _startTime = value;
            OnPropertyChanged();
        }
    }

    private TimeSpan _endTime;

    /// <summary>زمان پایان.</summary>
    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime == value) return;
            _endTime = value;
            OnPropertyChanged();
        }
    }

    /// <summary>مرور اول - فقط‌خواندنی = Date + ۲۴ ساعت (۱ روز بعد).</summary>
    public DateTime ReviewDate1 => Date.AddDays(1);

    /// <summary>مرور دوم - فقط‌خواندنی = Date + ۷ روز بعد.</summary>
    public DateTime ReviewDate2 => Date.AddDays(7);

    /// <summary>مرور سوم - فقط‌خواندنی = Date + ۳۰ روز بعد.</summary>
    public DateTime ReviewDate3 => Date.AddDays(30);

    // Foreign key
    public int StudyDayId { get; set; }
    public StudyDay? StudyDay { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
