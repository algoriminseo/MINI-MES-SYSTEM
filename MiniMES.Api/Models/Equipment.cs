namespace MiniMES.Api.Models;

public class Equipment
{
    public int Id { get; set; }

    public string EquipmentCode { get; set; } = string.Empty;

    public string EquipmentName { get; set; } = string.Empty;

    public int ProcessId { get; set; }

    public ManufacturingProcess? Process { get; set; }

    public string Status { get; set; } = "Idle";

    public string? Location { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
