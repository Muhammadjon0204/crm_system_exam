using Domain.Models;

namespace Infrastructure.Interface;

public interface IProgressBookService
{
    Task<string> AddAsync(ProgressBook progressBook);
    Task<string> UpdateAsync(int id, ProgressBook progressBook);
    Task<List<ProgressBook>> GetByStudentIdAsync(int studentId);
    Task<List<ProgressBook>> GetByGroupIdAsync(int groupId);
}