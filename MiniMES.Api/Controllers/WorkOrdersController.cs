using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Data;
using MiniMES.Api.Dtos.WorkOrders;
using MiniMES.Api.Models;

namespace MiniMES.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrdersController(MiniMesDbContext dbContext) : ControllerBase
{
    private static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Planned",
        "Released",
        "InProgress",
        "Paused",
        "Completed",
        "Cancelled"
    };

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrder>>> GetWorkOrders(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] int? itemId,
        [FromQuery] bool includeInactive = false)
    {
        var query = dbContext.WorkOrders
            .Include(workOrder => workOrder.Item)
            .AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(workOrder => workOrder.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(workOrder => workOrder.Status == NormalizeStatus(status));
        }

        if (itemId.HasValue)
        {
            query = query.Where(workOrder => workOrder.ItemId == itemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(workOrder => workOrder.WorkOrderNo.Contains(search));
        }

        var workOrders = await query
            .OrderByDescending(workOrder => workOrder.CreatedAt)
            .ToListAsync();

        return Ok(workOrders);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkOrder>> GetWorkOrder(int id)
    {
        var workOrder = await dbContext.WorkOrders
            .Include(workOrder => workOrder.Item)
            .AsNoTracking()
            .FirstOrDefaultAsync(workOrder => workOrder.Id == id);

        if (workOrder is null)
        {
            return NotFound();
        }

        return Ok(workOrder);
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrder>> CreateWorkOrder(CreateWorkOrderRequest request)
    {
        if (request.OrderQuantity <= 0)
        {
            return BadRequest("OrderQuantity must be greater than zero.");
        }

        var itemExists = await dbContext.Items
            .AnyAsync(item => item.Id == request.ItemId && item.IsActive);

        if (!itemExists)
        {
            return BadRequest($"Active item id '{request.ItemId}' does not exist.");
        }

        var workOrderNo = string.IsNullOrWhiteSpace(request.WorkOrderNo)
            ? await GenerateWorkOrderNoAsync()
            : request.WorkOrderNo.Trim().ToUpperInvariant();

        var exists = await dbContext.WorkOrders
            .AnyAsync(workOrder => workOrder.WorkOrderNo == workOrderNo);

        if (exists)
        {
            return Conflict($"WorkOrderNo '{workOrderNo}' already exists.");
        }

        var workOrder = new WorkOrder
        {
            WorkOrderNo = workOrderNo,
            ItemId = request.ItemId,
            OrderQuantity = request.OrderQuantity,
            Status = "Planned",
            PlannedStartDate = request.PlannedStartDate,
            DueDate = request.DueDate,
            Remark = NormalizeOptionalText(request.Remark),
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkOrders.Add(workOrder);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWorkOrder), new { id = workOrder.Id }, workOrder);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorkOrder(int id, UpdateWorkOrderRequest request)
    {
        var workOrder = await dbContext.WorkOrders.FirstOrDefaultAsync(workOrder => workOrder.Id == id);

        if (workOrder is null)
        {
            return NotFound();
        }

        if (workOrder.Status is "Completed" or "Cancelled")
        {
            return BadRequest("Completed or cancelled work orders cannot be edited.");
        }

        if (request.OrderQuantity <= 0)
        {
            return BadRequest("OrderQuantity must be greater than zero.");
        }

        if (request.OrderQuantity < workOrder.ProducedQuantity + workOrder.DefectQuantity)
        {
            return BadRequest("OrderQuantity cannot be less than recorded production quantity.");
        }

        var itemExists = await dbContext.Items
            .AnyAsync(item => item.Id == request.ItemId && item.IsActive);

        if (!itemExists)
        {
            return BadRequest($"Active item id '{request.ItemId}' does not exist.");
        }

        workOrder.ItemId = request.ItemId;
        workOrder.OrderQuantity = request.OrderQuantity;
        workOrder.PlannedStartDate = request.PlannedStartDate;
        workOrder.DueDate = request.DueDate;
        workOrder.Remark = NormalizeOptionalText(request.Remark);
        workOrder.IsActive = request.IsActive;
        workOrder.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id:int}/release")]
    public Task<IActionResult> ReleaseWorkOrder(int id)
    {
        return ChangeStatusAsync(id, "Released", "Planned");
    }

    [HttpPatch("{id:int}/start")]
    public Task<IActionResult> StartWorkOrder(int id)
    {
        return ChangeStatusAsync(id, "InProgress", "Released", "Paused");
    }

    [HttpPatch("{id:int}/pause")]
    public Task<IActionResult> PauseWorkOrder(int id)
    {
        return ChangeStatusAsync(id, "Paused", "InProgress");
    }

    [HttpPatch("{id:int}/complete")]
    public Task<IActionResult> CompleteWorkOrder(int id)
    {
        return ChangeStatusAsync(id, "Completed", "InProgress");
    }

    [HttpPatch("{id:int}/cancel")]
    public Task<IActionResult> CancelWorkOrder(int id)
    {
        return ChangeStatusAsync(id, "Cancelled", "Planned", "Released", "Paused");
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteWorkOrder(int id)
    {
        var workOrder = await dbContext.WorkOrders.FirstOrDefaultAsync(workOrder => workOrder.Id == id);

        if (workOrder is null)
        {
            return NotFound();
        }

        if (workOrder.Status is "InProgress" or "Completed")
        {
            return BadRequest("In-progress or completed work orders cannot be deactivated.");
        }

        workOrder.IsActive = false;
        workOrder.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<IActionResult> ChangeStatusAsync(int id, string nextStatus, params string[] allowedCurrentStatuses)
    {
        if (!Statuses.Contains(nextStatus))
        {
            return BadRequest("Invalid work order status.");
        }

        var workOrder = await dbContext.WorkOrders.FirstOrDefaultAsync(workOrder => workOrder.Id == id);

        if (workOrder is null)
        {
            return NotFound();
        }

        if (!workOrder.IsActive)
        {
            return BadRequest("Inactive work orders cannot change status.");
        }

        if (!allowedCurrentStatuses.Contains(workOrder.Status, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest($"Cannot change status from '{workOrder.Status}' to '{nextStatus}'.");
        }

        workOrder.Status = nextStatus;
        workOrder.UpdatedAt = DateTime.UtcNow;

        if (nextStatus == "InProgress" && workOrder.StartedAt is null)
        {
            workOrder.StartedAt = DateTime.UtcNow;
        }

        if (nextStatus == "Completed")
        {
            workOrder.CompletedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private async Task<string> GenerateWorkOrderNoAsync()
    {
        var today = DateTime.Today;
        var prefix = $"WO-{today:yyyyMMdd}-";

        var count = await dbContext.WorkOrders
            .CountAsync(workOrder => workOrder.WorkOrderNo.StartsWith(prefix));

        return $"{prefix}{count + 1:000}";
    }

    private static string NormalizeStatus(string status)
    {
        var value = status.Trim().ToLowerInvariant();
        return value switch
        {
            "released" => "Released",
            "inprogress" => "InProgress",
            "paused" => "Paused",
            "completed" => "Completed",
            "cancelled" => "Cancelled",
            _ => "Planned"
        };
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
