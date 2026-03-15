using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ProgressBookService(DataContext _dataContext, ILogger<ProgressBookService> _logger) : IProgressBookService
{
    public async Task<string> AddAsync(ProgressBook progressBook)
    {
        _logger.LogInformation("Добавление записи в журнал: студент ID={StudentId}, группа ID={GroupId}",
            progressBook.StudentId, progressBook.GroupId);

        if (progressBook.Grade < 1 || progressBook.Grade > 5)
        {
            _logger.LogWarning("Некорректная оценка {Grade} для студента ID={StudentId}", progressBook.Grade, progressBook.StudentId);
            return "Grade must be between 1 and 5.";
        }

        if (progressBook.LateMinutes < 0 || progressBook.LateMinutes > 120)
        {
            _logger.LogWarning("Некорректное опоздание {Minutes} мин для студента ID={StudentId}", progressBook.LateMinutes, progressBook.StudentId);
            return "LateMinutes must be between 0 and 120.";
        }

        using var context = _dataContext.CreateConnection();

        var studentExists = await context.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM students WHERE id = @Id", new { Id = progressBook.StudentId });
        if (studentExists != 1)
        {
            _logger.LogWarning("Студент ID={StudentId} не найден при добавлении в журнал", progressBook.StudentId);
            return "Student not found.";
        }

        var groupExists = await context.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM groups WHERE id = @Id", new { Id = progressBook.GroupId });
        if (groupExists != 1)
        {
            _logger.LogWarning("Группа ID={GroupId} не найдена при добавлении в журнал", progressBook.GroupId);
            return "Group not found.";
        }

        var isMember = await context.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM studentgroups WHERE studentid = @StudentId AND groupid = @GroupId",
            new { progressBook.StudentId, progressBook.GroupId });
        if (isMember != 1)
        {
            _logger.LogWarning("Студент ID={StudentId} не состоит в группе ID={GroupId}", progressBook.StudentId, progressBook.GroupId);
            return "Student is not a member of this group.";
        }

        var hasTimetable = await context.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM timetable WHERE groupid = @GroupId", new { progressBook.GroupId });
        if (hasTimetable != 1)
        {
            _logger.LogWarning("У группы ID={GroupId} нет расписания", progressBook.GroupId);
            return "This group has no timetable. Please add a timetable entry first.";
        }

        var sql = """
                    INSERT INTO progressbook (grade, studentid, isattended, date, groupid, notes, lateminutes, updatebyuserid)
                    VALUES (@Grade, @StudentId, @IsAttended, @Date, @GroupId, @Notes, @LateMinutes, @UpdateByUserId)
                  """;
        try
        {
            var result = await context.ExecuteAsync(sql, progressBook);
            if (result == 0)
            {
                _logger.LogError("Не удалось добавить запись в журнал для студента ID={StudentId}", progressBook.StudentId);
                return "Failed to add progress entry.";
            }
            _logger.LogInformation("Запись в журнал добавлена: студент ID={StudentId}, оценка={Grade}, посещение={IsAttended}",
                progressBook.StudentId, progressBook.Grade, progressBook.IsAttended);
            return "Progress entry added successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при добавлении записи в журнал студента ID={StudentId}", progressBook.StudentId);
            throw;
        }
    }

    public async Task<string> UpdateAsync(int id, ProgressBook progressBook)
    {
        _logger.LogInformation("Обновление записи журнала ID={EntryId}", id);

        if (progressBook.Grade < 1 || progressBook.Grade > 5)
        {
            _logger.LogWarning("Некорректная оценка {Grade} при обновлении записи ID={EntryId}", progressBook.Grade, id);
            return "Grade must be between 1 and 5.";
        }

        if (progressBook.LateMinutes < 0 || progressBook.LateMinutes > 120)
        {
            _logger.LogWarning("Некорректное опоздание {Minutes} мин при обновлении записи ID={EntryId}", progressBook.LateMinutes, id);
            return "LateMinutes must be between 0 and 120.";
        }

        using var context = _dataContext.CreateConnection();
        var exists = await context.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM progressbook WHERE id = @Id", new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Запись журнала ID={EntryId} не найдена", id);
            return "Progress entry not found.";
        }

        var sql = """
                    UPDATE progressbook
                    SET grade = @Grade, isattended = @IsAttended, date = @Date,
                        notes = @Notes, lateminutes = @LateMinutes, updatebyuserid = @UpdateByUserId
                    WHERE id = @Id
                  """;
        try
        {
            var result = await context.ExecuteAsync(sql, new
            {
                Id = id,
                progressBook.Grade,
                progressBook.IsAttended,
                progressBook.Date,
                progressBook.Notes,
                progressBook.LateMinutes,
                progressBook.UpdateByUserId
            });
            if (result == 0)
            {
                _logger.LogError("Не удалось обновить запись журнала ID={EntryId}", id);
                return "Failed to update progress entry.";
            }
            _logger.LogInformation("Запись журнала ID={EntryId} успешно обновлена", id);
            return "Progress entry updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при обновлении записи журнала ID={EntryId}", id);
            throw;
        }
    }

    public async Task<List<ProgressBook>> GetByStudentIdAsync(int studentId)
    {
        _logger.LogInformation("Запрос журнала студента ID={StudentId}", studentId);
        using var context = _dataContext.CreateConnection();
        var sql = "SELECT * FROM progressbook WHERE studentid = @StudentId ORDER BY date DESC";
        var result = await context.QueryAsync<ProgressBook>(sql, new { StudentId = studentId });
        var list = result.ToList();
        _logger.LogInformation("Получено {Count} записей журнала для студента ID={StudentId}", list.Count, studentId);
        return list;
    }

    public async Task<List<ProgressBook>> GetByGroupIdAsync(int groupId)
    {
        _logger.LogInformation("Запрос журнала группы ID={GroupId}", groupId);
        using var context = _dataContext.CreateConnection();
        var sql = "SELECT * FROM progressbook WHERE groupid = @GroupId ORDER BY date DESC";
        var result = await context.QueryAsync<ProgressBook>(sql, new { GroupId = groupId });
        var list = result.ToList();
        _logger.LogInformation("Получено {Count} записей журнала для группы ID={GroupId}", list.Count, groupId);
        return list;
    }
}