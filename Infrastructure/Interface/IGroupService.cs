using Domain.Models;

namespace Infrastructure.Interface;

public interface IGroupService
{
    Task<List<Group>> GetAllAsync();
    Task<Group?> GetByIdAsync(int id);
    Task<string> AddAsync(Group group);
    Task<string> UpdateAsync(int id, Group group);
    Task<string> DeleteAsync(int id);
    Task<List<Student>> GetStudentsAsync(int groupId);
    Task<string> AddStudentAsync(int groupId, int studentId);
    Task<string> RemoveStudentAsync(int groupId, int studentId);
    Task<List<TopStudentDto>> GetTopStudentsAsync(int groupId);
}