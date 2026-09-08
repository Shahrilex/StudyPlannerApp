using StudyPlanner.Core.Models;

namespace StudyPlanner.App.ViewModels;

public class TopicSearchResult
{
    public StudyTopic Topic { get; }
    public string JalaliDateText { get; }
    public DateTime GregorianDate { get; }
    public string SubjectDisplayName { get; }

    public TopicSearchResult(StudyTopic topic, string jalaliDateText, DateTime gregorianDate)
    {
        Topic = topic;
        JalaliDateText = jalaliDateText;
        GregorianDate = gregorianDate;
        SubjectDisplayName = topic.Subject.ToPersianDisplayName();
    }
}
