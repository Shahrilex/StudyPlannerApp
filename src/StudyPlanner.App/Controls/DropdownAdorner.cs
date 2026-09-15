using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace StudyPlanner.App.Controls;

/// <summary>
/// Adorner ساده برای نمایش یک المان دلخواه (مثلاً پنل چک‌باکس‌ها) درست زیر یک
/// FrameworkElement - در همان پنجره (بدون Popup/HWND جدا)، پس هیچ باگ
/// focus/mouse-capture/layered-window ندارد؛ ورودی ماوس و کیبورد دقیقاً مثل
/// هر کنترل دیگری در همان صفحه به‌صورت عادی کار می‌کند.
/// </summary>
public class DropdownAdorner : Adorner
{
    private readonly UIElement _content;
    private readonly VisualCollection _visuals;

    public DropdownAdorner(UIElement adornedElement, UIElement content) : base(adornedElement)
    {
        _content = content;
        _visuals = new VisualCollection(this) { content };
        IsHitTestVisible = true;
        Focusable = false;
    }

    protected override int VisualChildrenCount => _visuals.Count;

    protected override Visual GetVisualChild(int index) => _visuals[index];

    protected override Size MeasureOverride(Size constraint)
    {
        _content.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return AdornedElement.RenderSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _content.Arrange(new Rect(
            0,
            AdornedElement.RenderSize.Height,
            _content.DesiredSize.Width,
            _content.DesiredSize.Height));
        return finalSize;
    }
}
