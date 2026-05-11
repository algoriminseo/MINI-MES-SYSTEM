using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Data;
using MiniMES.Api.Dtos.Equipments;
using MiniMES.Api.Models;

namespace MiniMES.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EquipmentsController(MiniMesDbContext dbContext) : ControllerBase
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Idle",
        "Running",
        "Down",
        "Maintenance"
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Equipment>>> GetEquipments(
        [FromQuery] string? search,
        [FromQuery] int? processId,
        [FromQuery] string? status,
        [FromQuery] bool includeInactive = false)
    {
        var query = dbContext.Equipments
            .Include(equipment => equipment.Process)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(equipment => equipment.IsActive);
        }

        if (processId.HasValue)
        {
            query = query.Where(equipment => equipment.ProcessId == processId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(equipment => equipment.Status == NormalizeStatus(status));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(equipment =>
                equipment.EquipmentCode.Contains(search) ||
                equipment.EquipmentName.Contains(search));
        }

        var equipments = await query
            .OrderBy(equipment => equipment.Process!.SortOrder)
            .ThenBy(equipment => equipment.EquipmentCode)
            .ToListAsync();

        return Ok(equipments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Equipment>> GetEquipment(int id)
    {
        var equipment = await dbContext.Equipments
            .Include(equipment => equipment.Process)
            .AsNoTracking()
            .FirstOrDefaultAsync(equipment => equipment.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        return Ok(equipment);
    }

    [HttpPost]
    public async Task<ActionResult<Equipment>> CreateEquipment(CreateEquipmentRequest request)
    {
        var equipmentCode = request.EquipmentCode.Trim().ToUpperInvariant();
        var equipmentName = request.EquipmentName.Trim();
        var normalizedStatus = NormalizeStatus(request.Status);

        if (string.IsNullOrWhiteSpace(equipmentCode) || string.IsNullOrWhiteSpace(equipmentName))
        {
            return BadRequest("EquipmentCode and EquipmentName are required.");
        }

        if (!ValidStatuses.Contains(normalizedStatus))
        {
            return BadRequest("Status must be one of: Idle, Running, Down, Maintenance.");
        }

        var processExists = await dbContext.Processes
            .AnyAsync(process => process.Id == request.ProcessId && process.IsActive);

        if (!processExists)
        {
            return BadRequest($"Active process id '{request.ProcessId}' does not exist.");
        }

        var exists = await dbContext.Equipments
            .AnyAsync(equipment => equipment.EquipmentCode == equipmentCode);

        if (exists)
        {
            return Conflict($"EquipmentCode '{equipmentCode}' already exists.");
        }

        var equipment = new Equipment
        {
            EquipmentCode = equipmentCode,
            EquipmentName = equipmentName,
            ProcessId = request.ProcessId,
            Status = normalizedStatus,
            Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Equipments.Add(equipment);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEquipment), new { id = equipment.Id }, equipment);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEquipment(int id, UpdateEquipmentRequest request)
    {
        var equipment = await dbContext.Equipments.FirstOrDefaultAsync(equipment => equipment.Id == id);
        var normalizedStatus = NormalizeStatus(request.Status);

        if (equipment is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.EquipmentName))
        {
            return BadRequest("EquipmentName is required.");
        }

        if (!ValidStatuses.Contains(normalizedStatus))
        {
            return BadRequest("Status must be one of: Idle, Running, Down, Maintenance.");
        }

        var processExists = await dbContext.Processes
            .AnyAsync(process => process.Id == request.ProcessId && process.IsActive);

        if (!processExists)
        {
            return BadRequest($"Active process id '{request.ProcessId}' does not exist.");
        }

        equipment.EquipmentName = request.EquipmentName.Trim();
        equipment.ProcessId = request.ProcessId;
        equipment.Status = normalizedStatus;
        equipment.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        equipment.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        equipment.IsActive = request.IsActive;
        equipment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateEquipmentStatus(int id, [FromBody] string status)
    {
        var normalizedStatus = NormalizeStatus(status);

        if (!ValidStatuses.Contains(normalizedStatus))
        {
            return BadRequest("Status must be one of: Idle, Running, Down, Maintenance.");
        }

        var equipment = await dbContext.Equipments.FirstOrDefaultAsync(equipment => equipment.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        equipment.Status = normalizedStatus;
        equipment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEquipment(int id)
    {
        var equipment = await dbContext.Equipments.FirstOrDefaultAsync(equipment => equipment.Id == id);

        if (equipment is null)
        {
            return NotFound();
        }

        equipment.IsActive = false;
        equipment.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "Idle";
        }

        var value = status.Trim().ToLowerInvariant();
        return value switch
        {
            "running" => "Running",
            "down" => "Down",
            "maintenance" => "Maintenance",
            _ => "Idle"
        };
    }
}
