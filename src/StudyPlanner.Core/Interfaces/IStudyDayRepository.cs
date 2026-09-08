using StudyPlanner.Core.Models;

namespace StudyPlanner.Core.Interfaces;

public interface IStudyDayRepository
{
    Task<List<StudyDay>> GetAllAsync();

    Task<StudyDay?> GetByIdAsync(int id);

    Task<StudyDay?> GetByGregorianDateAsync(DateTime gregorianDate);

    /// <summary>
    /// روزهایی که حداقل یک مبحث یا جلسه دارند - برای نشانگر روی تقویم شمسی.
    /// </summary>
    Task<List<DateTime>> GetDatesWithDataAsync();

    Task<StudyDay> AddAsync(StudyDay studyDay);

    Task UpdateAsync(StudyDay studyDay);

    Task DeleteAsync(int id);

    Task AddTopicAsync(StudyTopic topic);

    Task UpdateTopicAsync(StudyTopic topic);

    Task DeleteTopicAsync(int topicId);

    Task AddSessionAsync(StudySession session);

    Task UpdateSessionAsync(StudySession session);

    Task DeleteSessionAsync(int sessionId);

    /// <summary>
    /// نزدیک‌ترین روزِ قبل از تاریخ داده‌شده که حداقل یک مبحث دارد - برای «کپی مباحث از روز قبل».
    /// </summary>
    Task<StudyDay?> GetNearestPreviousDayWithTopicsAsync(DateTime beforeDate);

    /// <summary>
    /// بعد از افزودن/حذف مبحث، RowNumber همه‌ی مباحث آن روز را دوباره پشت‌سرهم می‌کند و ذخیره می‌کند.
    /// </summary>
    Task RenumberTopicsAsync(int studyDayId);
}
