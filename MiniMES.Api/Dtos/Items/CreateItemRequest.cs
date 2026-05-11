namespace MiniMES.Api.Dtos.Items;

public class CreateItemRequest
{
    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string? Specification { get; set; }

    public string Unit { get; set; } = "EA";

    public string ItemType { get; set; } = "Product";

    public int SafetyStock { get; set; }
}
