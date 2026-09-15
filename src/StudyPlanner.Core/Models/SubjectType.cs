namespace StudyPlanner.Core.Models;

/// <summary>
/// عنوان درس - لیست بسته‌ی ۹ گزینه‌ای مباحث آزمون وکالت/قضاوت.
/// </summary>
public enum SubjectType
{
    Civil,              // حقوق مدنی
    CivilProcedure,     // آیین دادرسی مدنی
    Commercial,         // حقوق تجارت
    CriminalGeneral,    // حقوق جزای عمومی
    CriminalSpecific,   // حقوق جزای اختصاصی
    CriminalProcedure,  // آیین دادرسی کیفری
    PrinciplesOfFiqh,   // اصول فقه
    FiqhTexts,          // متون فقه
    Constitutional      // حقوق اساسی
}

public static class SubjectTypeExtensions
{
    public static string ToPersianDisplayName(this SubjectType subject) => subject switch
    {
        SubjectType.Civil => "حقوق مدنی",
        SubjectType.CivilProcedure => "آیین دادرسی مدنی",
        SubjectType.Commercial => "حقوق تجارت",
        SubjectType.CriminalGeneral => "حقوق جزای عمومی",
        SubjectType.CriminalSpecific => "حقوق جزای اختصاصی",
        SubjectType.CriminalProcedure => "آیین دادرسی کیفری",
        SubjectType.PrinciplesOfFiqh => "اصول فقه",
        SubjectType.FiqhTexts => "متون فقه",
        SubjectType.Constitutional => "حقوق اساسی",
        _ => subject.ToString()
    };
}
