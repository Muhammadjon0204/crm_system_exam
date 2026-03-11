using Domain.Models;
using Infrastructure.Interface;
using Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("progress")]
public class ProgressBookController
{
    private readonly IProgressBookService _progressBookService = new ProgressBookService();

    [HttpPost]
    public async Task<string> AddAsync(ProgressBook progressBook)
    {
        return await _progressBookService.AddAsync(progressBook);
    }

    [HttpPut("{id:int}")]
    public async Task<string> UpdateAsync(int id, ProgressBook progressBook)
    {
        return await _progressBookService.UpdateAsync(id, progressBook);
    }
}