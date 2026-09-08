namespace StudyPlanner.App.ViewModels;

/// <summary>
/// جفت مقدار/نمایش برای پرکردن DataGridComboBoxColumn (ItemsSource).
/// </summary>
public class ComboOption<T>
{
    public T Value { get; }
    public string Display { get; }

    public ComboOption(T value, string display)
    {
        Value = value;
        Display = display;
    }
}
