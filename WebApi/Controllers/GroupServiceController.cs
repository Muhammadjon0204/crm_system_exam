using Domain.Models;
using Infrastructure.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("groups")]
public class GroupServiceController
{
    private readonly IGroupService _groupService = new GroupService();

    [HttpGet]
    public async Task<List<Group>> GetAllAsync()
    {
        return await _groupService.GetAllAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<object> GetByIdAsync(int id)
    {
        var group = await _groupService.GetByIdAsync(id);
        if (group == null) return new { message = "Group not found." };
        return group;
    }

    [HttpPost]
    public async Task<string> AddAsync(Group group)
    {
        return await _groupService.AddAsync(group);
    }

    [HttpPut("{id:int}")]
    public async Task<string> UpdateAsync(int id, Group group)
    {
        return await _groupService.UpdateAsync(id, group);
    }

    [HttpDelete("{id:int}")]
    public async Task<string> DeleteAsync(int id)
    {
        return await _groupService.DeleteAsync(id);
    }
    
    [HttpPost("{groupId:int}/students/{studentId:int}")]
    public async Task<string> AddStudentAsync(int groupId, int studentId)
    {
        return await _groupService.AddStudentAsync(groupId, studentId);
    }
    
    [HttpDelete("{groupId:int}/students/{studentId:int}")]
    public async Task<string> RemoveStudentAsync(int groupId, int studentId)
    {
        return await _groupService.RemoveStudentAsync(groupId, studentId);
    }
    
    [HttpGet("{groupId:int}/students")]
    public async Task<List<Student>> GetStudentsAsync(int groupId)
    {
        return await _groupService.GetStudentsAsync(groupId);
    }

    [HttpGet("{groupId:int}/timetable")]
    public async Task<List<TimeTable>> GetTimetableAsync(int groupId)
    {
        var timeTableService = new TimeTableService();
        return await timeTableService.GetByGroupIdAsync(groupId);
    }

    [HttpGet("{groupId:int}/progress")]
    public async Task<object> GetProgressAsync(int groupId)
    {
        var progressService = new ProgressBookService();
        return await progressService.GetByGroupIdAsync(groupId);
    }

    // BONUS
    [HttpGet("{groupId:int}/top-students")]
    public async Task<object> GetTopStudentsAsync(int groupId)
    {
        return await _groupService.GetTopStudentsAsync(groupId);
    }
}