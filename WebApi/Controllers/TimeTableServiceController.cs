using Domain.Models;
using Infrastructure.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/timetable")]
public class TimeTableServiceController(ITimeTableService _timeTableService)
{

    [HttpPost]
    public async Task<string> AddAsync(TimeTable timeTable)
    {
        return await _timeTableService.AddAsync(timeTable);
    }

    [HttpPut("{id:int}")]
    public async Task<string> UpdateAsync(int id, TimeTable timeTable)
    {
        return await _timeTableService.UpdateAsync(id, timeTable);
    }

    [HttpGet("{groupId:int}/timetable")]
    public async Task<List<TimeTable>> GetTimetableAsync(int groupId)
    {
        return await _timeTableService.GetByGroupIdAsync(groupId);
    }

    [HttpDelete("{id:int}")]
    public async Task<string> DeleteAsync(int id)
    {
        return await _timeTableService.DeleteAsync(id);
    }
}