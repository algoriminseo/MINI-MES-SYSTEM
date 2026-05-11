using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMES.Api.Data;
using MiniMES.Api.Dtos.Items;
using MiniMES.Api.Models;

namespace MiniMES.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController(MiniMesDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetItems(
        [FromQuery] string? search,
        [FromQuery] bool includeInactive = false)
    {
        var query = dbContext.Items.AsNoTracking();

        if (!includeInactive)
        {
            query = query.Where(item => item.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(item =>
                item.ItemCode.Contains(search) ||
                item.ItemName.Contains(search));
        }

        var items = await query
            .OrderBy(item => item.ItemCode)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Item>> GetItem(int id)
    {
        var item = await dbContext.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<Item>> CreateItem(CreateItemRequest request)
    {
        var itemCode = request.ItemCode.Trim().ToUpperInvariant();
        var itemName = request.ItemName.Trim();

        if (string.IsNullOrWhiteSpace(itemCode) || string.IsNullOrWhiteSpace(itemName))
        {
            return BadRequest("ItemCode and ItemName are required.");
        }

        var exists = await dbContext.Items
            .AnyAsync(item => item.ItemCode == itemCode);

        if (exists)
        {
            return Conflict($"ItemCode '{itemCode}' already exists.");
        }

        var item = new Item
        {
            ItemCode = itemCode,
            ItemName = itemName,
            Specification = string.IsNullOrWhiteSpace(request.Specification) ? null : request.Specification.Trim(),
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? "EA" : request.Unit.Trim().ToUpperInvariant(),
            ItemType = string.IsNullOrWhiteSpace(request.ItemType) ? "Product" : request.ItemType.Trim(),
            SafetyStock = request.SafetyStock,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();

        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, UpdateItemRequest request)
    {
        var item = await dbContext.Items.FirstOrDefaultAsync(item => item.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.ItemName))
        {
            return BadRequest("ItemName is required.");
        }

        item.ItemName = request.ItemName.Trim();
        item.Specification = string.IsNullOrWhiteSpace(request.Specification) ? null : request.Specification.Trim();
        item.Unit = string.IsNullOrWhiteSpace(request.Unit) ? "EA" : request.Unit.Trim().ToUpperInvariant();
        item.ItemType = string.IsNullOrWhiteSpace(request.ItemType) ? "Product" : request.ItemType.Trim();
        item.SafetyStock = request.SafetyStock;
        item.IsActive = request.IsActive;
        item.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await dbContext.Items.FirstOrDefaultAsync(item => item.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        item.IsActive = false;
        item.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }
}
