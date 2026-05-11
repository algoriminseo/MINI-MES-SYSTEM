using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Data;
using MiniMES.Api.Dtos.Processes;
using MiniMES.Api.Models;

namespace MiniMES.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessesController(MiniMesDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingProcess>>> GetProcesses(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false)
    {
        var query = dbContext.Processes.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(process => process.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(process =>
                process.ProcessCode.Contains(search) ||
                process.ProcessName.Contains(search));
        }

        var processes = await query
            .OrderBy(process => process.SortOrder)
            .ThenBy(process => process.ProcessCode)
            .ToListAsync();

        return Ok(processes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ManufacturingProcess>> GetProcess(int id)
    {
        var process = await dbContext.Processes
            .AsNoTracking()
            .FirstOrDefaultAsync(process => process.Id == id);

        if (process is null)
        {
            return NotFound();
        }

        return Ok(process);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingProcess>> CreateProcess(CreateProcessRequest request)
    {
        var processCode = request.ProcessCode.Trim().ToUpperInvariant();
        var processName = request.ProcessName.Trim();

        if (string.IsNullOrWhiteSpace(processCode) || string.IsNullOrWhiteSpace(processName))
        {
            return BadRequest("ProcessCode and ProcessName are required.");
        }

        var exists = await dbContext.Processes
            .AnyAsync(process => process.ProcessCode == processCode);

        if (exists)
        {
            return Conflict($"ProcessCode '{processCode}' already exists.");
        }

        var process = new ManufacturingProcess
        {
            ProcessCode = processCode,
            ProcessName = processName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SortOrder = request.SortOrder,
            IsInspection = request.IsInspection,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Processes.Add(process);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProcess), new { id = process.Id }, process);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProcess(int id, UpdateProcessRequest request)
    {
        var process = await dbContext.Processes.FirstOrDefaultAsync(process => process.Id == id);

        if (process is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.ProcessName))
        {
            return BadRequest("ProcessName is required.");
        }

        process.ProcessName = request.ProcessName.Trim();
        process.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        process.SortOrder = request.SortOrder;
        process.IsInspection = request.IsInspection;
        process.IsActive = request.IsActive;
        process.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProcess(int id)
    {
        var process = await dbContext.Processes.FirstOrDefaultAsync(process => process.Id == id);

        if (process is null)
        {
            return NotFound();
        }

        process.IsActive = false;
        process.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
