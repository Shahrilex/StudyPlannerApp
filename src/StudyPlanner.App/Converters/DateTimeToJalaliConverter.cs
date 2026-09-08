using System.Globalization;
using System.Windows.Data;

namespace StudyPlanner.App.Converters;

public class DateTimeToJalaliConverter : IValueConverter
{
    private static readonly PersianCalendar Calendar = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date) return string.Empty;

        var year = Calendar.GetYear(date);
        var month = Calendar.GetMonth(date);
        var day = Calendar.GetDayOfMonth(date);
        return $"{year:0000}/{month:00}/{day:00}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("این ستون فقط‌خواندنی است.");
    }
}
