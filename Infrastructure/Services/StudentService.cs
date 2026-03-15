using Dapper;
using Domain.Models;
using Infrastructure.Context;
using Infrastructure.Interface;
using Microsoft.Extensions.Logging;
namespace Infrastructure.Services;

public class StudentService(DataContext _dataContext, ILogger<StudentService> _logger) : IStudentService
{

    public async Task<List<Student>> GetAllAsync()
    {
        _logger.LogInformation("Запрос всех студентов из БД");  // ← INFO

        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM students";
        var students = await connection.QueryAsync<Student>(sql);
        var list = students.ToList();

        _logger.LogInformation("Получено {Count} студентов", list.Count);  // ← INFO с параметром
        return list;
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Поиск студента ID={StudentId}", id);

        using var connection = _dataContext.CreateConnection();
        var sql = "SELECT * FROM students WHERE id = @Id";
        var student = await connection.QuerySingleOrDefaultAsync<Student>(sql, new { Id = id });

        if (student == null)
            _logger.LogWarning("Студент ID={StudentId} не найден", id);  // ← WARN
        else
            _logger.LogInformation("Найден: {FirstName} {LastName}", student.FirstName, student.LastName);

        return student;
    }

    public async Task<string> AddAsync(Student student)
    {
        _logger.LogInformation("Добавление студента: {FirstName} {LastName}", student.FirstName, student.LastName);

        using var connection = _dataContext.CreateConnection();
        var sql = """
                    INSERT INTO students (firstname, lastname, phone, email, createddate)
                    VALUES (@FirstName, @LastName, @Phone, @Email, @CreatedDate)
                  """;
        student.CreatedDate = DateTime.UtcNow;

        try
        {
            var result = await connection.ExecuteAsync(sql, student);
            if (result == 0)
            {
                _logger.LogError("Не удалось добавить студента {FirstName} {LastName}", student.FirstName, student.LastName);  // ← ERROR
                return "Failed to add student.";
            }

            _logger.LogInformation("Студент {FirstName} {LastName} успешно добавлен", student.FirstName, student.LastName);
            return "Student added successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при добавлении студента {FirstName} {LastName}", student.FirstName, student.LastName);  // ← ERROR + Exception
            throw;
        }
    }

    public async Task<string> UpdateAsync(int id, Student student)
    {
        _logger.LogInformation("Обновление студента ID={StudentId}", id);

        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Обновление отменено — студент ID={StudentId} не найден", id);
            return "Student not found.";
        }

        var sql = """
                    UPDATE students
                    SET firstname = @FirstName, lastname = @LastName, phone = @Phone, email = @Email
                    WHERE id = @Id
                  """;

        try
        {
            var result = await connection.ExecuteAsync(sql, new { Id = id, student.FirstName, student.LastName, student.Phone, student.Email });
            if (result == 0)
            {
                _logger.LogError("Не удалось обновить студента ID={StudentId}", id);
                return "Failed to update student.";
            }

            _logger.LogInformation("Студент ID={StudentId} успешно обновлён", id);
            return "Student updated successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при обновлении студента ID={StudentId}", id);
            throw;
        }
    }

    public async Task<string> DeleteAsync(int id)
    {
        _logger.LogWarning("Запрос на удаление студента ID={StudentId}", id);  // ← WARN (удаление — опасная операция)

        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Удаление отменено — студент ID={StudentId} не найден", id);
            return "Student not found.";
        }

        var sql = "DELETE FROM students WHERE id = @Id";

        try
        {
            var result = await connection.ExecuteAsync(sql, new { Id = id });
            if (result == 0)
            {
                _logger.LogError("Не удалось удалить студента ID={StudentId}", id);
                return "Failed to delete student.";
            }

            _logger.LogWarning("Студент ID={StudentId} удалён из БД", id);
            return "Student deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Исключение при удалении студента ID={StudentId}", id);
            throw;
        }
    }

    public async Task<AverageGradeDto> GetAverageGradeAsync(int id)
    {
        _logger.LogInformation("Запрос средней оценки для студента ID={StudentId}", id);

        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Средняя оценка: студент ID={StudentId} не найден", id);
            return new AverageGradeDto { Message = "Student not found." };
        }

        var sql = "SELECT ROUND(AVG(grade),2) FROM progressbook WHERE studentid = @Id";
        var avg = await connection.QuerySingleAsync<decimal>(sql, new { Id = id });
        var rounded = Math.Round(avg, 2);

        _logger.LogInformation("Средняя оценка студента ID={StudentId} = {Average}", id, rounded);
        return new AverageGradeDto { StudentId = id, AverageGrade = rounded };
    }

    public async Task<AttendanceDto> GetAttendanceAsync(int id)
    {
        _logger.LogInformation("Запрос посещаемости студента ID={StudentId}", id);

        using var connection = _dataContext.CreateConnection();
        var checkSql = "SELECT 1 FROM students WHERE id = @Id";
        var exists = await connection.QuerySingleOrDefaultAsync<int>(checkSql, new { Id = id });
        if (exists != 1)
        {
            _logger.LogWarning("Посещаемость: студент ID={StudentId} не найден", id);
            return new AttendanceDto { Message = "Student not found." };
        }

        var sql = """
                SELECT 
                    COUNT(*) AS total,
                    SUM(CASE WHEN isattended = TRUE THEN 1 ELSE 0 END) AS attended
                FROM progressbook
                WHERE studentid = @Id
              """;
        var raw = await connection.QuerySingleAsync<AttendanceRaw>(sql, new { Id = id });
        double percentage = raw.Total == 0 ? 0 : Math.Round((double)raw.Attended / raw.Total * 100, 2);

        _logger.LogInformation(
            "Посещаемость студента ID={StudentId}: {Attended}/{Total} = {Percentage}%",
            id, raw.Attended, raw.Total, percentage);

        if (percentage < 50)
            _logger.LogWarning("Низкая посещаемость у студента ID={StudentId}: {Percentage}%", id, percentage);

        return new AttendanceDto { StudentId = id, AttendancePercentage = percentage, Attended = raw.Attended, Total = raw.Total };
    }
}