namespace StudyPlanner.Core.Models;

/// <summary>
/// عنوان درس - لیست بسته‌ی ۹ گزینه‌ای مباحث آزمون وکالت/قضاوت.
/// </summary>
public enum SubjectType
{
    Civil,              // مدنی
    CivilProcedure,     // آیین دادرسی مدنی
    Commercial,         // تجارت
    CriminalGeneral,    // جزای عمومی
    CriminalSpecific,   // جزای اختصاصی
    CriminalProcedure,  // آیین دادرسی کیفری
    PrinciplesOfFiqh,   // اصول فقه
    FiqhTexts,          // متون فقه
    Constitutional      // اساسی
}

public static class SubjectTypeExtensions
{
    public static string ToPersianDisplayName(this SubjectType subject) => subject switch
    {
        SubjectType.Civil => "مدنی",
        SubjectType.CivilProcedure => "آیین دادرسی مدنی",
        SubjectType.Commercial => "تجارت",
        SubjectType.CriminalGeneral => "جزای عمومی",
        SubjectType.CriminalSpecific => "جزای اختصاصی",
        SubjectType.CriminalProcedure => "آیین دادرسی کیفری",
        SubjectType.PrinciplesOfFiqh => "اصول فقه",
        SubjectType.FiqhTexts => "متون فقه",
        SubjectType.Constitutional => "اساسی",
        _ => subject.ToString()
    };
}
