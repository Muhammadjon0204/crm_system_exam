using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;

namespace Infrastructure.Services;

public class ProgressBookService(DataContext _dataContext) : IProgressBookService
{

    public async Task<string> AddAsync(ProgressBook progressBook)
    {
        // 4
        if (progressBook.Grade < 1 || progressBook.Grade > 5)
            return "Grade must be between 1 and 5.";

        // 5
        if (progressBook.LateMinutes < 0 || progressBook.LateMinutes > 120)
            return "LateMinutes must be between 0 and 120.";

        using var context = _dataContext.CreateConnection();

        // 1
        var studentCheck = "SELECT 1 FROM students WHERE id = @Id";
        var studentExists =
            await context.QuerySingleOrDefaultAsync<int>(studentCheck, new { Id = progressBook.StudentId });
        if (studentExists != 1)
            return "Student not found.";

        var groupCheck = "SELECT 1 FROM groups WHERE id = @Id";
        var groupExists = await context.QuerySingleOrDefaultAsync<int>(groupCheck, new { Id = progressBook.GroupId });
        if (groupExists != 1)
            return "Group not found.";

        // 2
        var memberCheck = "SELECT 1 FROM studentgroups WHERE studentid = @StudentId AND groupid = @GroupId";
        var isMember = await context.QuerySingleOrDefaultAsync<int>(memberCheck, new
        {
            progressBook.StudentId,
            progressBook.GroupId
        });
        if (isMember != 1)
            return "Student is not a member of this group.";

        // 3
        var timetableCheck = "SELECT 1 FROM timetable WHERE groupid = @GroupId";
        var hasTimetable = await context.QuerySingleOrDefaultAsync<int>(timetableCheck, new { progressBook.GroupId });
        if (hasTimetable != 1)
            return "This group has no timetable. Please add a timetable entry first.";

        var sql = """
                    INSERT INTO progressbook (grade, studentid, isattended, date, groupid, notes, lateminutes, updatebyuserid)
                    VALUES (@Grade, @StudentId, @IsAttended, @Date, @GroupId, @Notes, @LateMinutes, @UpdateByUserId)
                  """;
        var result = await context.ExecuteAsync(sql, progressBook);
        return result == 0
            ? "Failed to add progress entry."
            : "Progress entry added successfully.";
    }

    public async Task<string> UpdateAsync(int id, ProgressBook progressBook)
    {
        // 4
        if (progressBook.Grade < 1 || progressBook.Grade > 5)
            return "Grade must be between 1 and 5.";

        // 5
        if (progressBook.LateMinutes < 0 || progressBook.LateMinutes > 120)
            return "LateMinutes must be between 0 and 120.";

        using var context = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM progressbook WHERE id = @Id";
        var exists = await context.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return "Progress entry not found.";

        var sql = """
                    UPDATE progressbook
                    SET grade = @Grade, isattended = @IsAttended, date = @Date,
                        notes = @Notes, lateminutes = @LateMinutes, updatebyuserid = @UpdateByUserId
                    WHERE id = @Id
                  """;
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
        return result == 0
            ? "Failed to update progress entry."
            : "Progress entry updated successfully.";
    }

    public async Task<List<ProgressBook>> GetByStudentIdAsync(int studentId)
    {
        using var context = _dataContext.CreateConnection();
        var sql = "SELECT * FROM progressbook WHERE studentid = @StudentId ORDER BY date DESC";
        var result = await context.QueryAsync<ProgressBook>(sql, new { StudentId = studentId });
        return result.ToList();
    }

    public async Task<List<ProgressBook>> GetByGroupIdAsync(int groupId)
    {
        using var context = _dataContext.CreateConnection();
        var sql = "SELECT * FROM progressbook WHERE groupid = @GroupId ORDER BY date DESC";
        var result = await context.QueryAsync<ProgressBook>(sql, new { GroupId = groupId });
        return result.ToList();
    }
}