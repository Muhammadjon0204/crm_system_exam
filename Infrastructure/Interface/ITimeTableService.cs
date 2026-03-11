using Domain.Models;

namespace Infrastructure.Interface;

public interface ITimeTableService
{
    Task<List<TimeTable>> GetByGroupIdAsync(int groupId);
    Task<string> AddAsync(TimeTable timeTable);
    Task<string> UpdateAsync(int id, TimeTable timeTable);
    Task<string> DeleteAsync(int id);
}