using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;

namespace Infrastructure.Services;

public class TimeTableService : ITimeTableService
{
    private readonly DataContext _dataContext = new();

    public async Task<List<TimeTable>> GetByGroupIdAsync(int groupId)
    {
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM timetable WHERE groupid = @GroupId";
        var result = await connection.QueryAsync<TimeTable>(sql, new { GroupId = groupId });
        return result.ToList();
    }

    public async Task<string> AddAsync(TimeTable timeTable)
    {
        // 6
        if (timeTable.FromTime >= timeTable.ToTime)
            return "FromTime must be earlier than ToTime.";

        using var connection = _dataContext.CreateConnection();

        var groupCheck = "SELECT 1 FROM groups WHERE id = @Id";
        var groupExists = await connection.QuerySingleOrDefaultAsync<int>(groupCheck, new { Id = timeTable.GroupId });
        if (groupExists != 1)
            return "Group not found.";

        // 7
        var duplicateCheck = "SELECT 1 FROM timetable WHERE groupid = @GroupId AND dayofweek = @DayOfWeek";
        var duplicate = await connection.QuerySingleOrDefaultAsync<int>(duplicateCheck, new
        {
            timeTable.GroupId,
            timeTable.DayOfWeek
        });
        if (duplicate == 1)
            return "A timetable entry for this group and day already exists.";

        var sql = """
                    INSERT INTO timetable (dayofweek, fromtime, totime, createddate, updateddate, groupid)
                    VALUES (@DayOfWeek, @FromTime, @ToTime, @CreatedDate, @UpdatedDate, @GroupId)
                  """;
        timeTable.CreatedDate = DateTime.UtcNow;
        timeTable.UpdatedDate = DateTime.UtcNow;
        var result = await connection.ExecuteAsync(sql, timeTable);
        return result == 0
            ? "Failed to add timetable entry."
            : "Timetable entry added successfully.";
    }

    public async Task<string> UpdateAsync(int id, TimeTable timeTable)
    {
        // 6
        if (timeTable.FromTime >= timeTable.ToTime)
            return "FromTime must be earlier than ToTime.";

        using var connection = _dataContext.CreateConnection();

        var checkSql = "SELECT groupid, dayofweek FROM timetable WHERE id = @Id";
        var existing = await connection.QuerySingleOrDefaultAsync(checkSql, new { Id = id });
        if (existing == null)
            return "Timetable entry not found.";

        // 7
        var duplicateCheck = """
                               SELECT 1 FROM timetable 
                               WHERE groupid = @GroupId AND dayofweek = @DayOfWeek AND id != @Id
                             """;
        var duplicate = await connection.QuerySingleOrDefaultAsync<int>(duplicateCheck, new
        {
            timeTable.GroupId,
            timeTable.DayOfWeek,
            Id = id
        });
        if (duplicate == 1)
            return "A timetable entry for this group and day already exists.";

        var sql = """
                    UPDATE timetable
                    SET dayofweek = @DayOfWeek, fromtime = @FromTime, totime = @ToTime, 
                        updateddate = @UpdatedDate, groupid = @GroupId
                    WHERE id = @Id
                  """;
        var result = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            timeTable.DayOfWeek,
            timeTable.FromTime,
            timeTable.ToTime,
            UpdatedDate = DateTime.UtcNow,
            timeTable.GroupId
        });
        return result == 0
            ? "Failed to update timetable entry."
            : "Timetable entry updated successfully.";
    }

    public async Task<string> DeleteAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();

        var checkSql = "SELECT 1 FROM timetable WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return "Timetable entry not found.";

        var sql = "DELETE FROM timetable WHERE id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result == 0
            ? "Failed to delete timetable entry."
            : "Timetable entry deleted successfully.";
    }
}