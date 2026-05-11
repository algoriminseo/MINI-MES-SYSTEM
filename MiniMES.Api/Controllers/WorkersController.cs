using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Data;
using MiniMES.Api.Dtos.Workers;
using MiniMES.Api.Models;

namespace MiniMES.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkersController(MiniMesDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Worker>>> GetWorkers(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] string? shiftGroup,
        [FromQuery] bool includeInactive = false)
    {
        var query = dbContext.Workers.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(worker => worker.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(worker => worker.Department == department.Trim());
        }

        if (!string.IsNullOrWhiteSpace(shiftGroup))
        {
            query = query.Where(worker => worker.ShiftGroup == shiftGroup.Trim().ToUpperInvariant());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(worker =>
                worker.WorkerCode.Contains(search) ||
                worker.WorkerName.Contains(search));
        }

        var workers = await query
            .OrderBy(worker => worker.WorkerCode)
            .ToListAsync();

        return Ok(workers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Worker>> GetWorker(int id)
    {
        var worker = await dbContext.Workers
            .AsNoTracking()
            .FirstOrDefaultAsync(worker => worker.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        return Ok(worker);
    }

    [HttpPost]
    public async Task<ActionResult<Worker>> CreateWorker(CreateWorkerRequest request)
    {
        var workerCode = request.WorkerCode.Trim().ToUpperInvariant();
        var workerName = request.WorkerName.Trim();

        if (string.IsNullOrWhiteSpace(workerCode) || string.IsNullOrWhiteSpace(workerName))
        {
            return BadRequest("WorkerCode and WorkerName are required.");
        }

        var exists = await dbContext.Workers
            .AnyAsync(worker => worker.WorkerCode == workerCode);

        if (exists)
        {
            return Conflict($"WorkerCode '{workerCode}' already exists.");
        }

        var worker = new Worker
        {
            WorkerCode = workerCode,
            WorkerName = workerName,
            Department = NormalizeOptionalText(request.Department),
            Role = NormalizeOptionalText(request.Role),
            ShiftGroup = NormalizeShiftGroup(request.ShiftGroup),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Workers.Add(worker);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorker), new { id = worker.Id }, worker);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorker(int id, UpdateWorkerRequest request)
    {
        var worker = await dbContext.Workers.FirstOrDefaultAsync(worker => worker.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.WorkerName))
        {
            return BadRequest("WorkerName is required.");
        }

        worker.WorkerName = request.WorkerName.Trim();
        worker.Department = NormalizeOptionalText(request.Department);
        worker.Role = NormalizeOptionalText(request.Role);
        worker.ShiftGroup = NormalizeShiftGroup(request.ShiftGroup);
        worker.IsActive = request.IsActive;
        worker.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWorker(int id)
    {
        var worker = await dbContext.Workers.FirstOrDefaultAsync(worker => worker.Id == id);

        if (worker is null)
        {
            return NotFound();
        }

        worker.IsActive = false;
        worker.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeShiftGroup(string? shiftGroup)
    {
        return string.IsNullOrWhiteSpace(shiftGroup) ? null : shiftGroup.Trim().ToUpperInvariant();
    }
}
