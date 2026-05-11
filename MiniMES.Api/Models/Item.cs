namespace MiniMES.Api.Models;

public class Item
{
    public int Id { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string? Specification { get; set; }

    public string Unit { get; set; } = "EA";

    public string ItemType { get; set; } = "Product";

    public int SafetyStock { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
