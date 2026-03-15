using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TimeTableService(DataContext _dataContext, ILogger<TimeTableService> _logger) : ITimeTableService
{
    public async Task<List<TimeTable>> GetByGroupIdAsync(int groupId)
    {
        _logger.LogInformation("Запрос расписания группы ID={GroupId}", groupId);
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM timetable WHERE groupid = @GroupId";
        var result = await connection.QueryAsync<TimeTable>(sql, new { GroupId = groupId });
        var list = result.ToList();
        _logger.LogInformation("Найдено {Count} записей расписания для группы ID={GroupId}", list.Count, groupId);
        return list;
    }

    public async Task<string> AddAsync(TimeTable timeTable)
    {
        _logger.LogInformation("Добавление расписания для группы ID={GroupId}", timeTable.GroupId);
        if (timeTable.FromTime >= timeTable.ToTime)
        {
            _logger.LogWarning("Некорректное время: FromTime >= ToTime для группы ID={GroupId}", timeTable.GroupId);
            return "FromTime must be earlier than ToTime.";
        }

        using var connection = _dataContext.CreateConnection();
        var groupExists = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM groups WHERE id = @Id", new { Id = timeTable.GroupId });
        if (groupExists != 1)
        {
            _logger.LogWarning("Группа ID={GroupId} не найдена при добавлении расписания", timeTable.GroupId);
            return "Group not found.";
        }

        var duplicate = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM timetable WHERE groupid = @GroupId AND dayofweek = @DayOfWeek",
            new { timeTable.GroupId, timeTable.DayOfWeek });
        if (duplicate == 1)
        {
            _logger.LogWarning("Расписание для группы ID={GroupId} на день {Day} уже существует", timeTable.GroupId, timeTable.DayOfWeek);
            return "A timetable entry for this group and day already exists.";
        }

        var sql = """
                    INSERT INTO timetable (dayofweek, fromtime, totime, createddate, updateddate, groupid)
                    VALUES (@DayOfWeek, @FromTime, @ToTime, @CreatedDate, @UpdatedDate, @GroupId)
                  """;
        timeTable.CreatedDate = DateTime.UtcNow;
        timeTable.UpdatedDate = DateTime.UtcNow;

        try
        {
            var result = await connection.ExecuteAsync(sql, timeTable);
            if (result == 0)
            {
                _logger.LogError("Не удалось добавить расписание для группы ID={GroupId}", timeTable.GroupId);
                return "Failed to add timetable entry.";
            }
            _logger.LogInformation("Расписание для группы ID={GroupId} успешно добавлено", timeTable.GroupId);
            return "Timetable entry added successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при добавлении расписания группы ID={GroupId}", timeTable.GroupId);
            throw;
        }
    }

    public async Task<string> UpdateAsync(int id, TimeTable timeTable)
    {
        _logger.LogInformation("Обновление расписания ID={TimetableId}", id);
        if (timeTable.FromTime >= timeTable.ToTime)
        {
            _logger.LogWarning("Некорректное время при обновлении расписания ID={TimetableId}", id);
            return "FromTime must be earlier than ToTime.";
        }

        using var connection = _dataContext.CreateConnection();
        var existing = await connection.QuerySingleOrDefaultAsync(
            "SELECT groupid, dayofweek FROM timetable WHERE id = @Id", new { Id = id });
        if (existing == null)
        {
            _logger.LogWarning("Расписание ID={TimetableId} не найдено", id);
            return "Timetable entry not found.";
        }

        var duplicate = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM timetable WHERE groupid = @GroupId AND dayofweek = @DayOfWeek AND id != @Id",
            new { timeTable.GroupId, timeTable.DayOfWeek, Id = id });
        if (duplicate == 1)
        {
            _logger.LogWarning("Конфликт расписания: группа ID={GroupId} день {Day} уже занят", timeTable.GroupId, timeTable.DayOfWeek);
            return "A timetable entry for this group and day already exists.";
        }

        var sql = """
                    UPDATE timetable
                    SET dayofweek = @DayOfWeek, fromtime = @FromTime, totime = @ToTime,
                        updateddate = @UpdatedDate, groupid = @GroupId
                    WHERE id = @Id
                  """;
        try
        {
            var result = await connection.ExecuteAsync(sql, new
            {
                Id = id,
                timeTable.DayOfWeek,
                timeTable.FromTime,
                timeTable.ToTime,
                UpdatedDate = DateTime.UtcNow,
                timeTable.GroupId
            });
            if (result == 0)
            {
                _logger.LogError("Не удалось обновить расписание ID={TimetableId}", id);
                return "Failed to update timetable entry.";
            }
            _logger.LogInformation("Расписание ID={TimetableId} успешно обновлено", id);
            return "Timetable entry updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при обновлении расписания ID={TimetableId}", id);
            throw;
        }
    }

    public async Task<string> DeleteAsync(int id)
    {
        _logger.LogWarning("Запрос на удаление расписания ID={TimetableId}", id);
        using var connection = _dataContext.CreateConnection();
        var exists = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM timetable WHERE id = @Id", new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Расписание ID={TimetableId} не найдено", id);
            return "Timetable entry not found.";
        }

        try
        {
            var result = await connection.ExecuteAsync(
                "DELETE FROM timetable WHERE id = @Id", new { Id = id });
            if (result == 0)
            {
                _logger.LogError("Не удалось удалить расписание ID={TimetableId}", id);
                return "Failed to delete timetable entry.";
            }
            _logger.LogWarning("Расписание ID={TimetableId} удалено", id);
            return "Timetable entry deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при удалении расписания ID={TimetableId}", id);
            throw;
        }
    }
}