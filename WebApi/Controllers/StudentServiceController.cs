using Domain.Models;
using Infrastructure.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/students")]
public class StudentServiceController(IStudentService _studentService)
{

    [HttpGet]
    public async Task<List<Student>> GetAllAsync()
    {
        return await _studentService.GetAllAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<object> GetByIdAsync(int id)
    {
        var student = await _studentService.GetByIdAsync(id);
        if (student == null) return new { message = "Student not found." };
        return student;
    }

    [HttpPost]
    public async Task<string> AddAsync(Student student)
    {
        return await _studentService.AddAsync(student);
    }

    [HttpPut("{id:int}")]
    public async Task<string> UpdateAsync(int id, Student student)
    {
        return await _studentService.UpdateAsync(id, student);
    }

    [HttpDelete("{id:int}")]
    public async Task<string> DeleteAsync(int id)
    {
        return await _studentService.DeleteAsync(id);
    }


    //bonus
    [HttpGet("{id:int}/average-grade")]
    public async Task<object> GetAverageGradeAsync(int id)
    {
        return await _studentService.GetAverageGradeAsync(id);
    }

    //bonus
    [HttpGet("{id:int}/attendance")]
    public async Task<AttendanceDto> GetAttendanceAsync(int id)
    {
        return await _studentService.GetAttendanceAsync(id);
    }
}