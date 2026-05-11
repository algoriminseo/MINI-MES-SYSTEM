namespace MiniMES.Api.Dtos.WorkOrders;

public class UpdateWorkOrderRequest
{
    public int ItemId { get; set; }

    public int OrderQuantity { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Remark { get; set; }

    public bool IsActive { get; set; } = true;
}
