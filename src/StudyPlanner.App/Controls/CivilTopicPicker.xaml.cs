using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using StudyPlanner.Core.Models;

namespace StudyPlanner.App.Controls;

/// <summary>
/// انتخاب‌گر چندگزینه‌ای مباحث مدنی همراه با شماره مواد.
///
/// چرا بدون Popup؟ نسخه‌ی قبلی از Popup با AllowsTransparency="True" استفاده
/// می‌کرد که یک پنجره‌ی ویندوزی جدا (layered window) می‌سازد. این نوع پنجره‌ها
/// یک باگ قدیمی و شناخته‌شده در WPF دارند: وقتی داخل یک DataGridTemplateColumn
/// باز می‌شوند، ماوس/کیبورد می‌تواند توسط سلول DataGrid capture شده بماند و
/// اصلاً به پنجره‌ی Popup نرسد - نتیجه: نه کلیک روی چک‌باکس‌ها اثر می‌کرد، نه
/// Tab/Space. چون این مشکل در سطح HWND/پیام‌های ویندوز رخ می‌دهد، هیچ کد
/// WPF‌ای (StaysOpen، PreviewMouseDown و غیره) نمی‌توانست قطعی حلش کند.
///
/// راه‌حل: هیچ پنجره‌ی جدایی ساخته نمی‌شود. لیست چک‌باکس‌ها با یک Adorner
/// مستقیماً در همان پنجره‌ی اصلی (روی لایه‌ی adorner که همیشه بالای همه چیز
/// است) رندر می‌شود. از نظر WPF این دقیقاً مثل هر کنترل دیگری در همان صفحه
/// است، پس مشکلات focus/mouse-capture/layered-window از اساس امکان وقوع ندارند.
/// </summary>
public partial class CivilTopicPicker : UserControl
{
    private DropdownAdorner? _adorner;
    private StackPanel? _optionsPanel;
    private bool _isLoading;

    public CivilTopicPicker()
    {
        InitializeComponent();
        Unloaded += (_, _) => CloseDropdown(save: false);
    }

    public static readonly DependencyProperty TopicProperty = DependencyProperty.Register(
        nameof(Topic), typeof(string), typeof(CivilTopicPicker),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectionTextChanged));

    public static readonly DependencyProperty ArticleNumbersProperty = DependencyProperty.Register(
        nameof(ArticleNumbers), typeof(string), typeof(CivilTopicPicker),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SelectedKeysProperty = DependencyProperty.Register(
        nameof(SelectedKeys), typeof(string), typeof(CivilTopicPicker),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SelectedSubjectProperty = DependencyProperty.Register(
        nameof(SelectedSubject), typeof(SubjectType), typeof(CivilTopicPicker), new PropertyMetadata(SubjectType.Civil));

    public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
        nameof(DisplayText), typeof(string), typeof(CivilTopicPicker), new PropertyMetadata("انتخاب مبحث/مباحث مدنی"));

    public string Topic { get => (string)GetValue(TopicProperty); set => SetValue(TopicProperty, value); }
    public string ArticleNumbers { get => (string)GetValue(ArticleNumbersProperty); set => SetValue(ArticleNumbersProperty, value); }
    public string SelectedKeys { get => (string)GetValue(SelectedKeysProperty); set => SetValue(SelectedKeysProperty, value); }
    public SubjectType SelectedSubject { get => (SubjectType)GetValue(SelectedSubjectProperty); set => SetValue(SelectedSubjectProperty, value); }

    public string DisplayText { get => (string)GetValue(DisplayTextProperty); private set => SetValue(DisplayTextProperty, value); }

    public event EventHandler? SelectionSaved;

    private static void OnSelectionTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((CivilTopicPicker)d).DisplayText = string.IsNullOrWhiteSpace((string?)e.NewValue) ? "انتخاب مبحث/مباحث مدنی" : (string)e.NewValue;

    private void OpenPicker_Click(object sender, RoutedEventArgs e)
    {
        // اگر همین چک‌باکس‌ها از قبل باز است، با کلیک دوباره روی دکمه ببندش (toggle).
        if (_adorner is not null)
        {
            CloseDropdown(save: true);
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(this);
        if (layer is null) return; // نباید پیش بیاید - هر عنصر داخل یک Window لایه‌ی adorner دارد.

        var content = BuildDropdownContent();
        _adorner = new DropdownAdorner(this, content);
        layer.Add(_adorner);

        if (Window.GetWindow(this) is Window window)
            window.PreviewMouseDown += Window_PreviewMouseDown;

        // فوکوس کیبورد را به اولین چک‌باکس می‌بریم تا بلافاصله با Tab/Space هم قابل‌استفاده باشد.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            _optionsPanel?.Children.OfType<CheckBox>().FirstOrDefault()?.Focus();
        }), System.Windows.Threading.DispatcherPriority.Input);
    }

    private UIElement BuildDropdownContent()
    {
        var selected = SelectedKeys.Split(';', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        _optionsPanel = new StackPanel();
        _isLoading = true;
        foreach (var option in CivilTopicCatalog.GetOptions(SelectedSubject))
        {
            var checkBox = new CheckBox
            {
                Content = option.Title,
                Tag = option,
                IsChecked = selected.Contains(option.Key),
                Margin = new Thickness(2, 3, 2, 3)
            };
            checkBox.Checked += TopicCheckBox_Changed;
            checkBox.Unchecked += TopicCheckBox_Changed;
            _optionsPanel.Children.Add(checkBox);
        }
        _isLoading = false;

        var confirmButton = new Button
        {
            Content = "تأیید",
            Margin = new Thickness(0, 8, 0, 0),
            Padding = new Thickness(8, 4, 8, 4)
        };
        confirmButton.Click += (_, _) => CloseDropdown(save: true);

        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Content = _optionsPanel,
            MaxHeight = 380
        };

        var dockPanel = new DockPanel();
        DockPanel.SetDock(confirmButton, Dock.Bottom);
        dockPanel.Children.Add(confirmButton);
        dockPanel.Children.Add(scrollViewer);

        return new Border
        {
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x1F, 0x3B, 0x57)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8),
            Width = 360,
            Effect = new DropShadowEffect { BlurRadius = 14, ShadowDepth = 2, Opacity = 0.35 },
            Child = dockPanel
        };
    }

    private void TopicCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoading) return;
        ApplySelection();
        // با هر تیک/برداشتن تیک بلافاصله ذخیره می‌شود - نیازی به «تأیید» برای هر تغییر نیست،
        // ولی اگر کاربر با کلیک بیرون یا دکمه‌ی تأیید ببندد، آخرین وضعیت هم دوباره ذخیره می‌شود.
        SelectionSaved?.Invoke(this, EventArgs.Empty);
    }

    private void ApplySelection()
    {
        if (_optionsPanel is null) return;

        var selected = new List<CivilTopicOption>();
        foreach (var checkBox in _optionsPanel.Children.OfType<CheckBox>())
        {
            if (checkBox.Tag is CivilTopicOption option && checkBox.IsChecked == true)
                selected.Add(option);
        }

        SelectedKeys = string.Join(';', selected.Select(x => x.Key));
        Topic = string.Join("، ", selected.Select(x => x.Title));
        ArticleNumbers = string.Join("، ", selected.Select(x => x.ArticleNumbers));
    }

    private void CloseDropdown(bool save)
    {
        if (_adorner is null) return;

        if (save) ApplySelection();

        var layer = AdornerLayer.GetAdornerLayer(this);
        layer?.Remove(_adorner);
        _adorner = null;
        _optionsPanel = null;

        if (Window.GetWindow(this) is Window window)
            window.PreviewMouseDown -= Window_PreviewMouseDown;

        if (save) SelectionSaved?.Invoke(this, EventArgs.Empty);
    }

    private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_adorner is null) return;

        if (e.OriginalSource is DependencyObject source &&
            (IsDescendant(_adorner, source) || IsDescendant(OpenButton, source)))
        {
            return; // کلیک داخل لیست یا روی دکمه‌ی بازکننده - چیزی نبند.
        }

        CloseDropdown(save: true);
    }

    private static bool IsDescendant(DependencyObject? root, DependencyObject node)
    {
        var current = node;
        while (current != null)
        {
            if (current == root) return true;
            current = LogicalTreeHelper.GetParent(current) ?? VisualTreeHelper.GetParent(current);
        }
        return false;
    }
}
