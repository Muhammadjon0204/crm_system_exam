using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class GroupService(DataContext _dataContext, ILogger<GroupService> _logger) : IGroupService
{
    public async Task<List<Group>> GetAllAsync()
    {
        _logger.LogInformation("Запрос всех групп");
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM groups";
        var groups = await connection.QueryAsync<Group>(sql);
        var list = groups.ToList();
        _logger.LogInformation("Получено {Count} групп", list.Count);
        return list;
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Поиск группы ID={GroupId}", id);
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM groups WHERE id = @Id";
        var group = await connection.QuerySingleOrDefaultAsync<Group>(sql, new { Id = id });
        if (group == null)
            _logger.LogWarning("Группа ID={GroupId} не найдена", id);
        else
            _logger.LogInformation("Группа найдена: {Name}", group.Name);
        return group;
    }

    public async Task<string> AddAsync(Group group)
    {
        _logger.LogInformation("Добавление группы: {Name}", group.Name);
        if (group.StartDate >= group.EndDate)
        {
            _logger.LogWarning("Некорректные даты группы {Name}: StartDate >= EndDate", group.Name);
            return "StartDate must be earlier than EndDate.";
        }

        using var connection = _dataContext.CreateConnection();
        group.CreatedDate = DateTime.UtcNow;
        var sql = """
                    INSERT INTO groups (name, startdate, enddate, createddate)
                    VALUES (@Name, @StartDate, @EndDate, @CreatedDate)
                  """;
        try
        {
            var result = await connection.ExecuteAsync(sql, group);
            if (result == 0)
            {
                _logger.LogError("Не удалось добавить группу {Name}", group.Name);
                return "Failed to add group.";
            }
            _logger.LogInformation("Группа {Name} успешно добавлена", group.Name);
            return "Group added successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при добавлении группы {Name}", group.Name);
            throw;
        }
    }

    public async Task<string> UpdateAsync(int id, Group group)
    {
        _logger.LogInformation("Обновление группы ID={GroupId}", id);
        if (group.StartDate >= group.EndDate)
        {
            _logger.LogWarning("Некорректные даты при обновлении группы ID={GroupId}", id);
            return "StartDate must be earlier than EndDate.";
        }

        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM groups WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Обновление отменено — группа ID={GroupId} не найдена", id);
            return "Group not found.";
        }

        var sql = """
                    UPDATE groups
                    SET name = @Name, startdate = @StartDate, enddate = @EndDate
                    WHERE id = @Id
                  """;
        try
        {
            var result = await connection.ExecuteAsync(sql, new { Id = id, group.Name, group.StartDate, group.EndDate });
            if (result == 0)
            {
                _logger.LogError("Не удалось обновить группу ID={GroupId}", id);
                return "Failed to update group.";
            }
            _logger.LogInformation("Группа ID={GroupId} успешно обновлена", id);
            return "Group updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при обновлении группы ID={GroupId}", id);
            throw;
        }
    }

    public async Task<string> DeleteAsync(int id)
    {
        _logger.LogWarning("Запрос на удаление группы ID={GroupId}", id);
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM groups WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Удаление отменено — группа ID={GroupId} не найдена", id);
            return "Group not found.";
        }

        var sql = "DELETE FROM groups WHERE id = @Id";
        try
        {
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            if (result == 0)
            {
                _logger.LogError("Не удалось удалить группу ID={GroupId}", id);
                return "Failed to delete group.";
            }
            _logger.LogWarning("Группа ID={GroupId} удалена", id);
            return "Group deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при удалении группы ID={GroupId}", id);
            throw;
        }
    }

    public async Task<List<Student>> GetStudentsAsync(int groupId)
    {
        _logger.LogInformation("Запрос студентов группы ID={GroupId}", groupId);
        using var connection = _dataContext.CreateConnection();
        var sql = """
                    SELECT s.* FROM students s
                    INNER JOIN studentgroups sg ON sg.studentid = s.id
                    WHERE sg.groupid = @GroupId
                  """;
        var students = await connection.QueryAsync<Student>(sql, new { GroupId = groupId });
        var list = students.ToList();
        _logger.LogInformation("В группе ID={GroupId} найдено {Count} студентов", groupId, list.Count);
        return list;
    }

    public async Task<string> AddStudentAsync(int groupId, int studentId)
    {
        _logger.LogInformation("Добавление студента ID={StudentId} в группу ID={GroupId}", studentId, groupId);
        using var connection = _dataContext.CreateConnection();

        var studentExists = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM students WHERE id = @Id", new { Id = studentId });
        if (studentExists != 1)
        {
            _logger.LogWarning("Студент ID={StudentId} не найден", studentId);
            return "Student not found.";
        }

        var groupExists = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM groups WHERE id = @Id", new { Id = groupId });
        if (groupExists != 1)
        {
            _logger.LogWarning("Группа ID={GroupId} не найдена", groupId);
            return "Group not found.";
        }

        var alreadyIn = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM studentgroups WHERE studentid = @StudentId AND groupid = @GroupId",
            new { StudentId = studentId, GroupId = groupId });
        if (alreadyIn == 1)
        {
            _logger.LogWarning("Студент ID={StudentId} уже состоит в группе ID={GroupId}", studentId, groupId);
            return "Student is already in this group.";
        }

        var sql = """
                    INSERT INTO studentgroups (studentid, groupid, joineddate)
                    VALUES (@StudentId, @GroupId, @JoinedDate)
                  """;
        var result = await connection.ExecuteAsync(sql, new { StudentId = studentId, GroupId = groupId, JoinedDate = DateTime.UtcNow });
        if (result == 0)
        {
            _logger.LogError("Не удалось добавить студента ID={StudentId} в группу ID={GroupId}", studentId, groupId);
            return "Failed to add student to group.";
        }
        _logger.LogInformation("Студент ID={StudentId} добавлен в группу ID={GroupId}", studentId, groupId);
        return "Student added to group successfully.";
    }

    public async Task<string> RemoveStudentAsync(int groupId, int studentId)
    {
        _logger.LogWarning("Удаление студента ID={StudentId} из группы ID={GroupId}", studentId, groupId);
        using var connection = _dataContext.CreateConnection();
        var exists = await connection.QuerySingleOrDefaultAsync<int>(
            "SELECT 1 FROM studentgroups WHERE groupid = @GroupId AND studentid = @StudentId",
            new { GroupId = groupId, StudentId = studentId });
        if (exists != 1)
        {
            _logger.LogWarning("Студент ID={StudentId} не состоит в группе ID={GroupId}", studentId, groupId);
            return "Student is not in this group.";
        }

        var result = await connection.ExecuteAsync(
            "DELETE FROM studentgroups WHERE groupid = @GroupId AND studentid = @StudentId",
            new { GroupId = groupId, StudentId = studentId });
        if (result == 0)
        {
            _logger.LogError("Не удалось удалить студента ID={StudentId} из группы ID={GroupId}", studentId, groupId);
            return "Failed to remove student from group.";
        }
        _logger.LogWarning("Студент ID={StudentId} удалён из группы ID={GroupId}", studentId, groupId);
        return "Student removed from group successfully.";
    }

    public async Task<List<TopStudentDto>> GetTopStudentsAsync(int groupId)
    {
        _logger.LogInformation("Запрос топ студентов группы ID={GroupId}", groupId);
        using var connection = _dataContext.CreateConnection();
        var sql = """
                    SELECT s.id, s.firstname, s.lastname,
                           ROUND(AVG(pb.grade), 2) AS averagegrade,
                           ROUND(SUM(CASE WHEN pb.isattended THEN 1 ELSE 0 END) / COUNT(*) * 100, 2) AS attendancepercent
                    FROM students s
                    JOIN studentgroups sg ON sg.studentid = s.id
                    LEFT JOIN progressbook pb ON pb.studentid = s.id AND pb.groupid = @GroupId
                    WHERE sg.groupid = @GroupId
                    GROUP BY s.id, s.firstname, s.lastname
                    ORDER BY averagegrade DESC, attendancepercent DESC
                  """;
        var result = await connection.QueryAsync<TopStudentDto>(sql, new { GroupId = groupId });
        var list = result.ToList();
        _logger.LogInformation("Топ студентов группы ID={GroupId}: получено {Count} записей", groupId, list.Count);
        return list;
    }
}