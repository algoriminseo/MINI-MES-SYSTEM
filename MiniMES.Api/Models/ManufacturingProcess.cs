namespace MiniMES.Api.Models;

public class ManufacturingProcess
{
    public int Id { get; set; }

    public string ProcessCode { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsInspection { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
