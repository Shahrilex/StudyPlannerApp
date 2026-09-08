using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace StudyPlanner.App.Controls;

public partial class PersianDatePickerButton : UserControl
{
    private static readonly PersianCalendar JalaliCalendar = new();

    public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
        nameof(SelectedDate), typeof(DateTime), typeof(PersianDatePickerButton),
        new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateChanged));

    public DateTime SelectedDate
    {
        get => (DateTime)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public PersianDatePickerButton()
    {
        InitializeComponent();
        UpdateDisplayText();
        ToggleBtn.Checked += (_, _) => CalendarPopup.IsOpen = true;
    }

    public event EventHandler? DateChanged;

    private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PersianDatePickerButton control)
        {
            control.UpdateDisplayText();
            // با انتخاب روز، پاپ‌آپ بسته شود.
            control.ToggleBtn.IsChecked = false;
            control.DateChanged?.Invoke(control, EventArgs.Empty);
        }
    }

    private void UpdateDisplayText()
    {
        if (DisplayText is null) return;
        var y = JalaliCalendar.GetYear(SelectedDate);
        var m = JalaliCalendar.GetMonth(SelectedDate);
        var d = JalaliCalendar.GetDayOfMonth(SelectedDate);
        DisplayText.Text = $"{y:0000}/{m:00}/{d:00}";
    }
}
