using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace StudyPlanner.App.Controls;

/// <summary>
/// تقویم شمسی گرافیکی - مستقل، بدون وابستگی به DI، مستقیماً با PersianCalendar کار می‌کند.
/// </summary>
public partial class PersianCalendarControl : UserControl
{
    private static readonly PersianCalendar Calendar = new();
    private static readonly string[] WeekDayNames = { "ش", "ی", "د", "س", "چ", "پ", "ج" };
    private static readonly string[] MonthNames =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private int _displayedYear;
    private int _displayedMonth;

    public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
        nameof(SelectedDate), typeof(DateTime), typeof(PersianCalendarControl),
        new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

    public DateTime SelectedDate
    {
        get => (DateTime)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public static readonly DependencyProperty DatesWithDataProperty = DependencyProperty.Register(
        nameof(DatesWithData), typeof(System.Collections.IEnumerable), typeof(PersianCalendarControl),
        new PropertyMetadata(null, OnDatesWithDataChanged));

    public System.Collections.IEnumerable? DatesWithData
    {
        get => (System.Collections.IEnumerable?)GetValue(DatesWithDataProperty);
        set => SetValue(DatesWithDataProperty, value);
    }

    public PersianCalendarControl()
    {
        InitializeComponent();
        BuildWeekDaysHeader();

        var today = DateTime.Today;
        _displayedYear = Calendar.GetYear(today);
        _displayedMonth = Calendar.GetMonth(today);

        Loaded += (_, _) => RenderMonth();
    }

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PersianCalendarControl control)
        {
            var date = (DateTime)e.NewValue;
            control._displayedYear = Calendar.GetYear(date);
            control._displayedMonth = Calendar.GetMonth(date);
            control.RenderMonth();
        }
    }

    private static void OnDatesWithDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PersianCalendarControl control)
        {
            control.RenderMonth();
        }
    }

    private void BuildWeekDaysHeader()
    {
        WeekDaysHeader.Children.Clear();
        foreach (var name in WeekDayNames)
        {
            WeekDaysHeader.Children.Add(new TextBlock
            {
                Text = name,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                FontSize = 15,
                Margin = new Thickness(2)
            });
        }
    }

    private void RenderMonth()
    {
        if (DaysGrid is null) return;

        HeaderText.Text = $"{MonthNames[_displayedMonth - 1]} {_displayedYear}";
        DaysGrid.Children.Clear();

        var daysInMonth = Calendar.GetDaysInMonth(_displayedYear, _displayedMonth);
        var firstOfMonthGregorian = Calendar.ToDateTime(_displayedYear, _displayedMonth, 1, 0, 0, 0, 0);

        // شنبه = شروع هفته‌ی شمسی (ستون اول)
        var firstDayOfWeek = ((int)firstOfMonthGregorian.DayOfWeek + 1) % 7; // Saturday -> 0

        var datesWithData = new HashSet<DateTime>();
        if (DatesWithData is not null)
        {
            foreach (var item in DatesWithData)
            {
                if (item is DateTime dt) datesWithData.Add(dt.Date);
            }
        }

        for (var i = 0; i < firstDayOfWeek; i++)
        {
            DaysGrid.Children.Add(new Border());
        }

        for (var day = 1; day <= daysInMonth; day++)
        {
            var gregorianDate = Calendar.ToDateTime(_displayedYear, _displayedMonth, day, 0, 0, 0, 0);
            var isToday = gregorianDate.Date == DateTime.Today;
            var isSelected = gregorianDate.Date == SelectedDate.Date;
            var hasData = datesWithData.Contains(gregorianDate.Date);

            var dayPanel = new StackPanel { Margin = new Thickness(2) };
            dayPanel.Children.Add(new TextBlock
            {
                Text = day.ToString(),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            if (hasData)
            {
                dayPanel.Children.Add(new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.DarkOrange,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 0)
                });
            }

            var button = new Button
            {
                Content = dayPanel,
                Padding = new Thickness(4),
                Margin = new Thickness(3),
                MinWidth = 90,
                MinHeight = 64,
                Tag = gregorianDate,
                Background = isSelected ? Brushes.SteelBlue : (isToday ? Brushes.LightGoldenrodYellow : Brushes.White),
                Foreground = isSelected ? Brushes.White : Brushes.Black,
                BorderBrush = isToday ? Brushes.DarkGoldenrod : Brushes.LightGray
            };
            button.Click += DayButton_Click;

            DaysGrid.Children.Add(button);
        }
    }

    private void DayButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: DateTime date })
        {
            SelectedDate = date;
        }
    }

    private void PreviousMonth_Click(object sender, RoutedEventArgs e)
    {
        _displayedMonth--;
        if (_displayedMonth < 1)
        {
            _displayedMonth = 12;
            _displayedYear--;
        }
        RenderMonth();
    }

    private void NextMonth_Click(object sender, RoutedEventArgs e)
    {
        _displayedMonth++;
        if (_displayedMonth > 12)
        {
            _displayedMonth = 1;
            _displayedYear++;
        }
        RenderMonth();
    }
}
