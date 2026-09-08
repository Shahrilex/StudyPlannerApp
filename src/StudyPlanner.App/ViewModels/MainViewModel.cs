using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using StudyPlanner.Core.Interfaces;
using StudyPlanner.Core.Models;

namespace StudyPlanner.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IStudyDayRepository _repository;
    private readonly IPersianDateService _persianDateService;
    private readonly IExcelExportService _excelExportService;
    private CancellationTokenSource? _toastCts;

    public ObservableCollection<StudyTopic> Topics { get; } = new();
    public ObservableCollection<StudySession> Sessions { get; } = new();
    public ObservableCollection<TopicSearchResult> SearchResults { get; } = new();
    public ObservableCollection<SubjectStat> SubjectStats { get; } = new();
    public ObservableCollection<MasteryStat> MasteryStats { get; } = new();

    /// <summary>درخواست به View برای تعویض تب (۰=تقویم، ۱=مبحث، ۲=جلسات، ۳=جست‌وجو، ۴=آمار).</summary>
    public event Action<int>? RequestTabSwitch;

    public List<ComboOption<SubjectType>> SubjectOptions { get; } =
        Enum.GetValues<SubjectType>().Select(s => new ComboOption<SubjectType>(s, s.ToPersianDisplayName())).ToList();

    public List<ComboOption<PriorityLevel>> PriorityOptions { get; } =
        Enum.GetValues<PriorityLevel>().Select(p => new ComboOption<PriorityLevel>(p, p.ToPersianDisplayName())).ToList();

    public List<ComboOption<MasteryLevel>> MasteryOptions { get; } =
        Enum.GetValues<MasteryLevel>().Select(m => new ComboOption<MasteryLevel>(m, m.ToPersianDisplayName())).ToList();

    [ObservableProperty]
    private ObservableCollection<DateTime> _datesWithData = new();

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;

    [ObservableProperty]
    private StudyDay? _currentDay;

    [ObservableProperty]
    private string _statusText = "یک روز را از تقویم انتخاب کنید.";

    [ObservableProperty]
    private string? _toastMessage;

    [ObservableProperty]
    private string? _todayReviewText;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _totalMinutesAllTime;

    public MainViewModel(
        IStudyDayRepository repository,
        IPersianDateService persianDateService,
        IExcelExportService excelExportService)
    {
        _repository = repository;
        _persianDateService = persianDateService;
        _excelExportService = excelExportService;

        _ = InitializeSafelyAsync();
    }

    private async Task InitializeSafelyAsync()
    {
        try
        {
            await InitializeAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا هنگام بارگذاری اطلاعات اولیه:\n\n{ex}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task InitializeAsync()
    {
        await RefreshDatesWithDataAsync();
        await LoadDayAsync(SelectedDate);
        await RefreshStatsAsync();
        await RefreshTodayReviewsAsync();
    }

    partial void OnSelectedDateChanged(DateTime value)
    {
        _ = LoadDayAsync(value);
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = RunSearchAsync(value);
    }

    private async Task RefreshDatesWithDataAsync()
    {
        var dates = await _repository.GetDatesWithDataAsync();
        DatesWithData = new ObservableCollection<DateTime>(dates);
    }

    private async Task LoadDayAsync(DateTime date)
    {
        var day = await _repository.GetByGregorianDateAsync(date.Date);
        CurrentDay = day;

        Topics.Clear();
        Sessions.Clear();

        if (day is null)
        {
            StatusText = $"روز {_persianDateService.ToJalaliText(date)} هنوز ساخته نشده - «ساخت روز جدید» را بزنید.";
            return;
        }

        foreach (var topic in day.Topics.OrderBy(t => t.RowNumber))
            Topics.Add(topic);

        foreach (var session in day.Sessions.OrderBy(s => s.Date))
            Sessions.Add(session);

        StatusText = $"روز {day.JalaliDateText} - {Topics.Count} مبحث، {Sessions.Count} جلسه.";
    }

    // ---------- تقویم / روز ----------

    [RelayCommand]
    private async Task CreateNewDay()
    {
        if (CurrentDay is not null)
        {
            ShowToast("این روز از قبل وجود دارد.");
            return;
        }

        var newDay = new StudyDay
        {
            GregorianDate = SelectedDate.Date,
            JalaliDateText = _persianDateService.ToJalaliText(SelectedDate)
        };

        await _repository.AddAsync(newDay);
        await RefreshDatesWithDataAsync();
        await LoadDayAsync(SelectedDate);
        ShowToast("روز جدید ساخته شد.");
    }

    // ---------- مباحث ----------

    [RelayCommand]
    private async Task AddTopic()
    {
        if (CurrentDay is null)
        {
            ShowToast("اول باید روز را بسازید.");
            return;
        }

        var topic = new StudyTopic
        {
            StudyDayId = CurrentDay.Id,
            Subject = SubjectType.Civil,
            Topic = string.Empty,
            Priority = PriorityLevel.Medium,
            Mastery = MasteryLevel.Weak,
            Notes = string.Empty
        };

        await _repository.AddTopicAsync(topic);
        Topics.Add(topic);
        await RenumberLocalTopicsAsync();
        await RefreshDatesWithDataAsync();
        await RefreshStatsAsync();
    }

    [RelayCommand]
    private async Task CopyTopicsFromPreviousDay()
    {
        if (CurrentDay is null)
        {
            ShowToast("اول باید روز را بسازید.");
            return;
        }

        var previousDay = await _repository.GetNearestPreviousDayWithTopicsAsync(CurrentDay.GregorianDate);
        if (previousDay is null)
        {
            ShowToast("هیچ روز قبلی با مبحث پیدا نشد.");
            return;
        }

        var confirm = MessageBox.Show(
            $"{previousDay.Topics.Count} مبحث از روز {previousDay.JalaliDateText} کپی شود؟ " +
            "(فقط عنوان درس/ساعت/اولویت/سطح تسلط کپی می‌شود؛ متن مبحث و توضیحات همیشه خالی شروع می‌شود)",
            "تأیید کپی", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes) return;

        foreach (var source in previousDay.Topics.OrderBy(t => t.RowNumber))
        {
            var copy = new StudyTopic
            {
                StudyDayId = CurrentDay.Id,
                Subject = source.Subject,
                Topic = string.Empty,      // طبق تصمیم اولیه: مبحث و توضیحات همیشه خالی شروع می‌شود
                StartTime = source.StartTime,
                EndTime = source.EndTime,
                Priority = source.Priority,
                Mastery = source.Mastery,
                Notes = string.Empty
            };
            await _repository.AddTopicAsync(copy);
        }

        await RenumberLocalTopicsAsync();
        await RefreshDatesWithDataAsync();
        await RefreshStatsAsync();
        ShowToast($"{previousDay.Topics.Count} مبحث کپی شد.");
    }

    [RelayCommand]
    private async Task DeleteTopic(StudyTopic? topic)
    {
        if (topic is null) return;

        var label = string.IsNullOrWhiteSpace(topic.Topic) ? $"ردیف {topic.RowNumber}" : topic.Topic;
        var confirm = MessageBox.Show($"مبحث «{label}» حذف شود؟ این عمل قابل بازگشت نیست.",
            "تأیید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        await _repository.DeleteTopicAsync(topic.Id);
        Topics.Remove(topic);
        await RenumberLocalTopicsAsync();
        await RefreshDatesWithDataAsync();
        await RefreshStatsAsync();
        ShowToast("مبحث حذف شد.");
    }

    private async Task RenumberLocalTopicsAsync()
    {
        // بعد از افزودن/حذف، سرور RowNumber ها را دوباره شماره‌گذاری کرده -
        // لیست محلی را از دیتابیس تازه می‌کنیم تا هماهنگ بماند.
        if (CurrentDay is null) return;
        var refreshed = await _repository.GetByIdAsync(CurrentDay.Id);
        if (refreshed is null) return;

        Topics.Clear();
        foreach (var t in refreshed.Topics.OrderBy(t => t.RowNumber))
            Topics.Add(t);
    }

    public async Task SaveTopicAsync(StudyTopic topic)
    {
        if (topic.EndTime <= topic.StartTime)
            ShowToast("⚠ ساعت پایان باید بعد از ساعت شروع باشد - زمان مطالعه صفر ثبت شد.");

        await _repository.UpdateTopicAsync(topic);
        await RefreshStatsAsync();
    }

    // ---------- جلسات ----------

    [RelayCommand]
    private async Task AddSession()
    {
        if (CurrentDay is null)
        {
            ShowToast("اول باید روز را بسازید.");
            return;
        }

        var session = new StudySession
        {
            StudyDayId = CurrentDay.Id,
            Date = CurrentDay.GregorianDate,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };

        await _repository.AddSessionAsync(session);
        Sessions.Add(session);
        await RefreshDatesWithDataAsync();
        await RefreshTodayReviewsAsync();
    }

    [RelayCommand]
    private async Task DeleteSession(StudySession? session)
    {
        if (session is null) return;

        var confirm = MessageBox.Show($"جلسه‌ی تاریخ {_persianDateService.ToJalaliText(session.Date)} حذف شود؟ این عمل قابل بازگشت نیست.",
            "تأیید حذف", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        await _repository.DeleteSessionAsync(session.Id);
        Sessions.Remove(session);
        await RefreshDatesWithDataAsync();
        await RefreshTodayReviewsAsync();
        ShowToast("جلسه حذف شد.");
    }

    public async Task SaveSessionAsync(StudySession session)
    {
        if (session.EndTime <= session.StartTime)
            ShowToast("⚠ ساعت پایان جلسه باید بعد از ساعت شروع باشد.");

        await _repository.UpdateSessionAsync(session);
        await RefreshTodayReviewsAsync();
    }

    // ---------- خروجی اکسل ----------

    [RelayCommand]
    private void ExportCurrentDay()
    {
        if (CurrentDay is null)
        {
            ShowToast("روزی برای خروجی گرفتن انتخاب نشده.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "فایل اکسل (*.xlsx)|*.xlsx",
            FileName = $"StudyPlanner_{CurrentDay.JalaliDateText.Replace("/", "-")}.xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            _excelExportService.ExportSingleDay(CurrentDay, dialog.FileName);
            ShowToast($"خروجی اکسل روز {CurrentDay.JalaliDateText} ساخته شد.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در ساخت خروجی اکسل:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ExportAllDays()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "فایل اکسل (*.xlsx)|*.xlsx",
            FileName = "StudyPlanner_All.xlsx"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var allDays = await _repository.GetAllAsync();
            _excelExportService.ExportAllDays(allDays, dialog.FileName);
            ShowToast($"خروجی اکسل همه‌ی {allDays.Count} روز ساخته شد.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در ساخت خروجی اکسل:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- پشتیبان‌گیری / بازیابی ----------

    [RelayCommand]
    private void BackupDatabase()
    {
        var dialog = new SaveFileDialog
        {
            Filter = "فایل پشتیبان (*.db)|*.db",
            FileName = $"StudyPlanner_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            // قبل از کپی، pool اتصال‌های باز را آزاد می‌کنیم تا فایل قفل نباشد.
            SqliteConnection.ClearAllPools();
            File.Copy(AppPaths.DbPath, dialog.FileName, overwrite: true);
            ShowToast("پشتیبان با موفقیت ذخیره شد.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در پشتیبان‌گیری:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RestoreDatabase()
    {
        var dialog = new OpenFileDialog { Filter = "فایل پشتیبان (*.db)|*.db" };
        if (dialog.ShowDialog() != true) return;

        var confirm = MessageBox.Show(
            "با بازیابی، تمام داده‌های فعلی با فایل پشتیبان جایگزین می‌شوند و برنامه بسته می‌شود تا دوباره بازش کنی. ادامه می‌دهید؟",
            "تأیید بازیابی", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        try
        {
            SqliteConnection.ClearAllPools();
            File.Copy(dialog.FileName, AppPaths.DbPath, overwrite: true);
            MessageBox.Show("بازیابی انجام شد. برنامه الان بسته می‌شود - دوباره بازش کن.",
                "بازیابی موفق", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در بازیابی:\n{ex.Message}", "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ---------- جست‌وجوی سراسری ----------

    private async Task RunSearchAsync(string query)
    {
        SearchResults.Clear();
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2) return;

        var allDays = await _repository.GetAllAsync();
        foreach (var day in allDays.OrderByDescending(d => d.GregorianDate))
        {
            foreach (var topic in day.Topics)
            {
                var matches = (topic.Topic?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (topic.Notes?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                    || topic.Subject.ToPersianDisplayName().Contains(query, StringComparison.OrdinalIgnoreCase);

                if (matches)
                    SearchResults.Add(new TopicSearchResult(topic, day.JalaliDateText, day.GregorianDate));
            }
        }
    }

    [RelayCommand]
    private void JumpToSearchResult(TopicSearchResult? result)
    {
        if (result is null) return;
        SelectedDate = result.GregorianDate;
        RequestTabSwitch?.Invoke(1); // تب مبحث مطالعه
    }

    // ---------- آمار / داشبورد ----------

    private async Task RefreshStatsAsync()
    {
        var allDays = await _repository.GetAllAsync();
        var allTopics = allDays.SelectMany(d => d.Topics).ToList();

        TotalMinutesAllTime = allTopics.Sum(t => t.AllocatedMinutes);

        SubjectStats.Clear();
        var subjectGroups = allTopics
            .GroupBy(t => t.Subject)
            .Select(g => new { Subject = g.Key, Minutes = g.Sum(t => t.AllocatedMinutes), Count = g.Count() })
            .OrderByDescending(x => x.Minutes)
            .ToList();
        var maxSubjectMinutes = subjectGroups.Count > 0 ? subjectGroups.Max(x => x.Minutes) : 0;

        foreach (var g in subjectGroups)
        {
            SubjectStats.Add(new SubjectStat
            {
                SubjectName = g.Subject.ToPersianDisplayName(),
                TotalMinutes = g.Minutes,
                TopicCount = g.Count,
                BarWidth = maxSubjectMinutes > 0 ? (double)g.Minutes / maxSubjectMinutes * 300 : 0
            });
        }

        MasteryStats.Clear();
        var masteryGroups = allTopics
            .GroupBy(t => t.Mastery)
            .Select(g => new { Mastery = g.Key, Count = g.Count() })
            .ToList();
        var maxMasteryCount = masteryGroups.Count > 0 ? masteryGroups.Max(x => x.Count) : 0;

        foreach (var g in masteryGroups)
        {
            MasteryStats.Add(new MasteryStat
            {
                MasteryName = g.Mastery.ToPersianDisplayName(),
                Count = g.Count,
                BarWidth = maxMasteryCount > 0 ? (double)g.Count / maxMasteryCount * 300 : 0
            });
        }
    }

    // ---------- یادآوری مرور امروز ----------

    private async Task RefreshTodayReviewsAsync()
    {
        var allDays = await _repository.GetAllAsync();
        var today = DateTime.Today;

        var dueCount = allDays.SelectMany(d => d.Sessions)
            .Count(s => s.ReviewDate1.Date == today || s.ReviewDate2.Date == today || s.ReviewDate3.Date == today);

        TodayReviewText = dueCount > 0 ? $"📌 امروز {dueCount} مرور برنامه‌ریزی‌شده داری." : null;
    }

    // ---------- Toast سبک ----------

    public void ShowToast(string message)
    {
        ToastMessage = message;
        _toastCts?.Cancel();
        _toastCts = new CancellationTokenSource();
        var token = _toastCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(3200, token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (!token.IsCancellationRequested)
            {
                Application.Current.Dispatcher.Invoke(() => ToastMessage = null);
            }
        }, token);
    }
}
