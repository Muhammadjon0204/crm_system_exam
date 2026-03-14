using Domain.Models;
using Infrastructure.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/progress")]
public class ProgressBookController(IProgressBookService _progressBookService)
{

    [HttpPost]
    public async Task<string> AddAsync(ProgressBook progressBook)
    {
        return await _progressBookService.AddAsync(progressBook);
    }

    [HttpGet("{group/id:int}/progress")]
    public async Task<object> GetProgressAsync(int groupId)
    {

        return await _progressBookService.GetByGroupIdAsync(groupId);
    }

    [HttpPut("{id:int}")]
    public async Task<string> UpdateAsync(int id, ProgressBook progressBook)
    {
        return await _progressBookService.UpdateAsync(id, progressBook);
    }
    [HttpGet("{student/id:int}/progress")]
    public async Task<object> GetProgressOfStusentAsync(int id)
    {
        return await _progressBookService.GetByStudentIdAsync(id);
    }
}