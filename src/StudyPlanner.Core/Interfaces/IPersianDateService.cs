namespace StudyPlanner.Core.Interfaces;

public interface IPersianDateService
{
    /// <summary>تبدیل تاریخ میلادی به متن شمسی برای نمایش، مثلاً «۱۴۰۴/۰۵/۰۴».</summary>
    string ToJalaliText(DateTime gregorianDate);

    /// <summary>تبدیل سال/ماه/روز شمسی به تاریخ میلادی.</summary>
    DateTime ToGregorian(int jalaliYear, int jalaliMonth, int jalaliDay);

    (int Year, int Month, int Day) ToJalali(DateTime gregorianDate);

    int GetDaysInJalaliMonth(int jalaliYear, int jalaliMonth);
}
