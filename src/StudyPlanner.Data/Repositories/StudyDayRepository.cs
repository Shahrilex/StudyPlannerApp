using Microsoft.EntityFrameworkCore;
using StudyPlanner.Core.Interfaces;
using StudyPlanner.Core.Models;

namespace StudyPlanner.Data.Repositories;

public class StudyDayRepository : IStudyDayRepository
{
    private readonly StudyPlannerDbContext _context;

    public StudyDayRepository(StudyPlannerDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudyDay>> GetAllAsync()
    {
        return await _context.StudyDays
            .Include(d => d.Topics)
            .Include(d => d.Sessions)
            .OrderBy(d => d.GregorianDate)
            .ToListAsync();
    }

    public async Task<StudyDay?> GetByIdAsync(int id)
    {
        return await _context.StudyDays
            .Include(d => d.Topics)
            .Include(d => d.Sessions)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<StudyDay?> GetByGregorianDateAsync(DateTime gregorianDate)
    {
        var dateOnly = gregorianDate.Date;
        return await _context.StudyDays
            .Include(d => d.Topics)
            .Include(d => d.Sessions)
            .FirstOrDefaultAsync(d => d.GregorianDate == dateOnly);
    }

    public async Task<List<DateTime>> GetDatesWithDataAsync()
    {
        return await _context.StudyDays
            .Where(d => d.Topics.Any() || d.Sessions.Any())
            .Select(d => d.GregorianDate)
            .ToListAsync();
    }

    public async Task<StudyDay> AddAsync(StudyDay studyDay)
    {
        _context.StudyDays.Add(studyDay);
        await _context.SaveChangesAsync();
        return studyDay;
    }

    public async Task UpdateAsync(StudyDay studyDay)
    {
        _context.StudyDays.Update(studyDay);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var day = await _context.StudyDays.FindAsync(id);
        if (day is null) return;

        _context.StudyDays.Remove(day);
        await _context.SaveChangesAsync();
    }

    public async Task AddTopicAsync(StudyTopic topic)
    {
        _context.StudyTopics.Add(topic);
        await _context.SaveChangesAsync();
        await RenumberTopicsAsync(topic.StudyDayId);
    }

    public async Task UpdateTopicAsync(StudyTopic topic)
    {
        _context.StudyTopics.Update(topic);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTopicAsync(int topicId)
    {
        var topic = await _context.StudyTopics.FindAsync(topicId);
        if (topic is null) return;

        var studyDayId = topic.StudyDayId;
        _context.StudyTopics.Remove(topic);
        await _context.SaveChangesAsync();
        await RenumberTopicsAsync(studyDayId);
    }

    public async Task AddSessionAsync(StudySession session)
    {
        _context.StudySessions.Add(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(StudySession session)
    {
        _context.StudySessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSessionAsync(int sessionId)
    {
        var session = await _context.StudySessions.FindAsync(sessionId);
        if (session is null) return;

        _context.StudySessions.Remove(session);
        await _context.SaveChangesAsync();
    }

    public async Task<StudyDay?> GetNearestPreviousDayWithTopicsAsync(DateTime beforeDate)
    {
        var dateOnly = beforeDate.Date;
        return await _context.StudyDays
            .Include(d => d.Topics)
            .Where(d => d.GregorianDate < dateOnly && d.Topics.Any())
            .OrderByDescending(d => d.GregorianDate)
            .FirstOrDefaultAsync();
    }

    public async Task RenumberTopicsAsync(int studyDayId)
    {
        var topics = await _context.StudyTopics
            .Where(t => t.StudyDayId == studyDayId)
            .OrderBy(t => t.Id)
            .ToListAsync();

        for (var i = 0; i < topics.Count; i++)
        {
            topics[i].RowNumber = i + 1;
        }

        await _context.SaveChangesAsync();
    }
}
