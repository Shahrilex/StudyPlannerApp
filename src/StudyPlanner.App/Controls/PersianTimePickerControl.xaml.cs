using System.Windows;
using System.Windows.Controls;

namespace StudyPlanner.App.Controls;

public partial class PersianTimePickerControl : UserControl
{
    private bool _suppressEvents;

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(TimeSpan), typeof(PersianTimePickerControl),
        new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public TimeSpan Value
    {
        get => (TimeSpan)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public PersianTimePickerControl()
    {
        InitializeComponent();

        for (var h = 0; h < 24; h++)
            HoursList.Items.Add(h.ToString("00"));

        for (var m = 0; m < 60; m += 5)
            MinutesList.Items.Add(m.ToString("00"));

        UpdateDisplayText();
        SyncListsFromValue();
    }

    public event EventHandler? ValueChanged;

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PersianTimePickerControl control)
        {
            control.UpdateDisplayText();
            control.SyncListsFromValue();
            control.ValueChanged?.Invoke(control, EventArgs.Empty);
        }
    }

    private void UpdateDisplayText()
    {
        if (DisplayText is null) return;
        DisplayText.Text = $"{Value.Hours:00}:{Value.Minutes:00}";
    }

    private void SyncListsFromValue()
    {
        if (HoursList is null || MinutesList is null) return;
        _suppressEvents = true;
        HoursList.SelectedItem = Value.Hours.ToString("00");
        MinutesList.SelectedItem = (Value.Minutes - Value.Minutes % 5).ToString("00");
        _suppressEvents = false;
    }

    private void HoursList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressEvents || HoursList.SelectedItem is null) return;
        var hour = int.Parse((string)HoursList.SelectedItem);
        Value = new TimeSpan(hour, Value.Minutes, 0);
    }

    private void MinutesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressEvents || MinutesList.SelectedItem is null) return;
        var minute = int.Parse((string)MinutesList.SelectedItem);
        Value = new TimeSpan(Value.Hours, minute, 0);
    }
}
