using Domain.Models;

namespace Infrastructure.Interface;

public interface IStudentService
{
    Task<List<Student>> GetAllAsync();
    Task<Student?> GetByIdAsync(int id);
    Task<string> AddAsync(Student student);
    Task<string> UpdateAsync(int id, Student student);
    Task<string> DeleteAsync(int id);
    Task<AverageGradeDto> GetAverageGradeAsync(int id);
    Task<AttendanceDto> GetAttendanceAsync(int id);
}