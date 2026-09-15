using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StudyPlanner.App.Controls;
using StudyPlanner.App.ViewModels;
using StudyPlanner.Core.Models;

namespace StudyPlanner.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += MainWindow_DataContextChanged;
    }

    private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is MainViewModel oldVm) oldVm.RequestTabSwitch -= OnRequestTabSwitch;
        if (e.NewValue is MainViewModel newVm) newVm.RequestTabSwitch += OnRequestTabSwitch;
    }

    private void OnRequestTabSwitch(int tabIndex)
    {
        if (tabIndex >= 0 && tabIndex < MainTabControl.Items.Count)
            MainTabControl.SelectedIndex = tabIndex;
    }

    private async void TopicsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (DataContext is not MainViewModel vm) return;
        if (e.Row.Item is not StudyTopic topic) return;

        // ادیت کامیت می‌شود اما مقدار هنوز روی آبجکت اعمال نشده - یک فریم صبر می‌کنیم.
        await Dispatcher.InvokeAsync(async () => await vm.SaveTopicAsync(topic),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    private async void SessionsGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit) return;
        if (DataContext is not MainViewModel vm) return;
        if (e.Row.Item is not StudySession session) return;

        await Dispatcher.InvokeAsync(async () => await vm.SaveSessionAsync(session),
            System.Windows.Threading.DispatcherPriority.Background);
    }

    // کنترل‌های تاریخ/ساعت سفارشی داخل ستون‌های Template خارج از چرخه‌ی عادی
    // CellEditEnding عمل می‌کنند، پس هرکدام هندلر ذخیره‌ی مستقیم خودشان را دارند.

    private async void TopicTimePicker_ValueChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (sender is not PersianTimePickerControl { DataContext: StudyTopic topic }) return;

        await vm.SaveTopicAsync(topic);
    }

    private async void CivilTopicPicker_SelectionSaved(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (sender is not CivilTopicPicker { DataContext: StudyTopic topic }) return;

        await vm.SaveTopicAsync(topic);
    }

    private async void SessionTimePicker_ValueChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (sender is not PersianTimePickerControl { DataContext: StudySession session }) return;

        await vm.SaveSessionAsync(session);
    }

    private async void SessionDatePicker_DateChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (sender is not PersianDatePickerButton { DataContext: StudySession session }) return;

        await vm.SaveSessionAsync(session);
    }

    // میانبر کیبورد: کلید Delete روی ردیف انتخاب‌شده همون تأیید حذف همیشگی رو نشون می‌ده.

    private void TopicsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        if (DataContext is not MainViewModel vm) return;
        if (TopicsGrid.SelectedItem is not StudyTopic topic) return;
        if (vm.DeleteTopicCommand.CanExecute(topic)) vm.DeleteTopicCommand.Execute(topic);
        e.Handled = true;
    }

    private void SessionsGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete) return;
        if (DataContext is not MainViewModel vm) return;
        if (SessionsGrid.SelectedItem is not StudySession session) return;
        if (vm.DeleteSessionCommand.CanExecute(session)) vm.DeleteSessionCommand.Execute(session);
        e.Handled = true;
    }

    // دابل‌کلیک روی نتیجه‌ی جست‌وجو -> رفتن به همون روز و تب مبحث.

    private void SearchResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        if (sender is not ListBoxItem { DataContext: TopicSearchResult result }) return;

        if (vm.JumpToSearchResultCommand.CanExecute(result)) vm.JumpToSearchResultCommand.Execute(result);
    }
}
