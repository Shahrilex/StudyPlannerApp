using System.Windows;

namespace StudyPlanner.App;

/// <summary>
/// DataGridColumn (و ItemsSource آن) عضو Visual/Logical Tree نیست، پس ElementName
/// همیشه قابل اعتماد نیست. Freezable ها مکانیزم InheritanceContext متفاوتی دارند که
/// این مشکل را قطعی حل می‌کند - این همان راه‌حل استاندارد و شناخته‌شده‌ی WPF است.
/// </summary>
public class BindingProxy : Freezable
{
    protected override Freezable CreateInstanceCore() => new BindingProxy();

    public object Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));
}
