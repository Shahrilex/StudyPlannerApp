using System.Globalization;
using StudyPlanner.Core.Interfaces;

namespace StudyPlanner.Services;

public class PersianDateService : IPersianDateService
{
    private readonly PersianCalendar _calendar = new();

    public string ToJalaliText(DateTime gregorianDate)
    {
        var (year, month, day) = ToJalali(gregorianDate);
        return $"{year:0000}/{month:00}/{day:00}";
    }

    public DateTime ToGregorian(int jalaliYear, int jalaliMonth, int jalaliDay)
    {
        return _calendar.ToDateTime(jalaliYear, jalaliMonth, jalaliDay, 0, 0, 0, 0);
    }

    public (int Year, int Month, int Day) ToJalali(DateTime gregorianDate)
    {
        var year = _calendar.GetYear(gregorianDate);
        var month = _calendar.GetMonth(gregorianDate);
        var day = _calendar.GetDayOfMonth(gregorianDate);
        return (year, month, day);
    }

    public int GetDaysInJalaliMonth(int jalaliYear, int jalaliMonth)
    {
        return _calendar.GetDaysInMonth(jalaliYear, jalaliMonth);
    }
}
