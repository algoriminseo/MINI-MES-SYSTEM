namespace MiniMES.Api.Models;

public class WorkOrder
{
    public int Id { get; set; }

    public string WorkOrderNo { get; set; } = string.Empty;

    public int ItemId { get; set; }

    public Item? Item { get; set; }

    public int OrderQuantity { get; set; }

    public int ProducedQuantity { get; set; }

    public int DefectQuantity { get; set; }

    public string Status { get; set; } = "Planned";

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Remark { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
