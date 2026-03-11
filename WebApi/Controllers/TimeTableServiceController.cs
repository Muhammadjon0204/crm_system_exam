using Domain.Models;
using Infrastructure.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("timetable")]
public class TimeTableServiceController
{
    private readonly ITimeTableService _timeTableService = new TimeTableService();

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

    [HttpDelete("{id:int}")]
    public async Task<string> DeleteAsync(int id)
    {
        return await _timeTableService.DeleteAsync(id);
    }
}