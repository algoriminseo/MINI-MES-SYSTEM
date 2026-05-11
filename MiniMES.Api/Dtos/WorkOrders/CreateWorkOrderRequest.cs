namespace MiniMES.Api.Dtos.WorkOrders;

public class CreateWorkOrderRequest
{
    public string? WorkOrderNo { get; set; }

    public int ItemId { get; set; }

    public int OrderQuantity { get; set; }

    public DateTime? PlannedStartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Remark { get; set; }
}
