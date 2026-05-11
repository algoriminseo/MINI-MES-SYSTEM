namespace MiniMES.Api.Dtos.Items;

public class UpdateItemRequest
{
    public string ItemName { get; set; } = string.Empty;

    public string? Specification { get; set; }

    public string Unit { get; set; } = "EA";

    public string ItemType { get; set; } = "Product";

    public int SafetyStock { get; set; }

    public bool IsActive { get; set; } = true;
}
