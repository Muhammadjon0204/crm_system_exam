using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;

namespace Infrastructure.Services;

public class GroupService(DataContext _dataContext) : IGroupService
{
    public async Task<List<Group>> GetAllAsync()
    {
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM groups";
        var groups = await connection.QueryAsync<Group>(sql);
        return groups.ToList();
    }

    public async Task<Group?> GetByIdAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM groups WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<Group>(sql, new { Id = id });
    }

    public async Task<string> AddAsync(Group group)
    {
        if (group.StartDate >= group.EndDate)
            return "StartDate must be earlier than EndDate.";

        using var connection = _dataContext.CreateConnection();
        group.CreatedDate = DateTime.UtcNow;
        var sql = """
                    INSERT INTO groups (name, startdate, enddate, createddate)
                    VALUES (@Name, @StartDate, @EndDate, @CreatedDate)
                  """;
        var result = await connection.ExecuteAsync(sql, group);
        return result == 0
            ? "Failed to add group."
            : "Group added successfully.";
    }

    public async Task<string> UpdateAsync(int id, Group group)
    {
        if (group.StartDate >= group.EndDate)
            return "StartDate must be earlier than EndDate.";

        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM groups WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return "Group not found.";

        var sql = """
                    UPDATE groups
                    SET name = @Name, startdate = @StartDate, enddate = @EndDate
                    WHERE id = @Id
                  """;
        var result = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            group.Name,
            group.StartDate,
            group.EndDate
        });
        return result == 0
            ? "Failed to update group."
            : "Group updated successfully.";
    }

    public async Task<string> DeleteAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM groups WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return "Group not found.";

        var sql = "DELETE FROM groups WHERE id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result == 0
            ? "Failed to delete group."
            : "Group deleted successfully.";
    }

    public async Task<List<Student>> GetStudentsAsync(int groupId)
    {
        using var connection = _dataContext.CreateConnection();
        var sql = """
                    SELECT s.* FROM students s
                    INNER JOIN studentgroups sg ON sg.studentid = s.id
                    WHERE sg.groupid = @GroupId
                  """;
        var students = await connection.QueryAsync<Student>(sql, new { GroupId = groupId });
        return students.ToList();
    }

    public async Task<string> AddStudentAsync(int groupId, int studentId)
    {
        using var connection = _dataContext.CreateConnection();

        var studentCheck = "SELECT 1 FROM students WHERE id = @Id";
        var studentExists = await connection.QuerySingleOrDefaultAsync<int>(studentCheck, new { Id = studentId });
        if (studentExists != 1)
            return "Student not found.";

        var groupCheck = "SELECT 1 FROM groups WHERE id = @Id";
        var groupExists = await connection.QuerySingleOrDefaultAsync<int>(groupCheck, new { Id = groupId });
        if (groupExists != 1)
            return "Group not found.";

        var duplicateCheck = "SELECT 1 FROM studentgroups WHERE studentid = @StudentId AND groupid = @GroupId";
        var alreadyIn =
            await connection.QuerySingleOrDefaultAsync<int>(duplicateCheck,
                new { StudentId = studentId, GroupId = groupId });
        if (alreadyIn == 1)
            return "Student is already in this group.";

        var sql = """
                    INSERT INTO studentgroups (studentid, groupid, joineddate)
                    VALUES (@StudentId, @GroupId, @JoinedDate)
                  """;
        var result = await connection.ExecuteAsync(sql, new
        {
            StudentId = studentId,
            GroupId = groupId,
            JoinedDate = DateTime.UtcNow
        });
        return result == 0
            ? "Failed to add student to group."
            : "Student added to group successfully.";
    }

    public async Task<string> RemoveStudentAsync(int groupId, int studentId)
    {
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM studentgroups WHERE groupid = @GroupId AND studentid = @StudentId";
        var exists =
            await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { GroupId = groupId, StudentId = studentId });
        if (exists != 1)
            return "Student is not in this group.";

        var sql = "DELETE FROM studentgroups WHERE groupid = @GroupId AND studentid = @StudentId";
        var result = await connection.ExecuteAsync(sql, new { GroupId = groupId, StudentId = studentId });
        return result == 0
            ? "Failed to remove student from group."
            : "Student removed from group successfully.";
    }

    public async Task<List<TopStudentDto>> GetTopStudentsAsync(int groupId)
    {
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
        return result.ToList();
    }
}