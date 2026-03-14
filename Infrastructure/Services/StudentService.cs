using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;

namespace Infrastructure.Services;

public class StudentService(DataContext _dataContext) : IStudentService
{

    public async Task<List<Student>> GetAllAsync()
    {
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM students";
        var students = await connection.QueryAsync<Student>(sql);
        return students.ToList();
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM students WHERE id = @Id";
        var student = await connection.QuerySingleOrDefaultAsync<Student>(sql, new { Id = id });
        return student;
    }

    public async Task<string> AddAsync(Student student)
    {
        using var connection = _dataContext.CreateConnection();
        var sql = """
                    INSERT INTO students (firstname, lastname, phone, email, createddate)
                    VALUES (@FirstName, @LastName, @Phone, @Email, @CreatedDate)
                  """;
        student.CreatedDate = DateTime.UtcNow;
        var result = await connection.ExecuteAsync(sql, student);
        return result == 0
            ? "Failed to add student."
            : "Student added successfully.";
    }

    public async Task<string> UpdateAsync(int id, Student student)
    {
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return "Student not found.";

        var sql = """
                    UPDATE students
                    SET firstname = @FirstName, lastname = @LastName, phone = @Phone, email = @Email
                    WHERE id = @Id
                  """;
        var result = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            student.FirstName,
            student.LastName,
            student.Phone,
            student.Email
        });
        return result == 0
            ? "Failed to update student."
            : "Student updated successfully.";
    }

    public async Task<string> DeleteAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return "Student not found.";

        var sql = "DELETE FROM students WHERE id = @Id";
        var result = await connection.ExecuteAsync(sql, new { Id = id });
        return result == 0
            ? "Failed to delete student."
            : "Student deleted successfully.";
    }

    public async Task<AverageGradeDto> GetAverageGradeAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return new AverageGradeDto { Message = "Student not found." };

        var sql = "SELECT ROUND(AVG(grade),2) FROM progressbook WHERE studentid = @Id";
        var avg = await connection.QuerySingleAsync<decimal>(sql, new { Id = id });
        return new AverageGradeDto { StudentId = id, AverageGrade = Math.Round(avg, 2) };
    }

    public async Task<AttendanceDto> GetAttendanceAsync(int id)
    {
        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
            return new AttendanceDto { Message = "Student not found." };

        var sql = """
                SELECT 
                    COUNT(*) AS total,
                    SUM(CASE WHEN isattended = TRUE THEN 1 ELSE 0 END) AS attended
                FROM progressbook
                WHERE studentid = @Id
              """;
        var raw = await connection.QuerySingleAsync<AttendanceRaw>(sql, new { Id = id });

        double percentage = raw.Total == 0
            ? 0
            : Math.Round((double)raw.Attended / raw.Total * 100, 2);

        return new AttendanceDto
        {
            StudentId = id,
            AttendancePercentage = percentage,
            Attended = raw.Attended,
            Total = raw.Total
        };
    }

}