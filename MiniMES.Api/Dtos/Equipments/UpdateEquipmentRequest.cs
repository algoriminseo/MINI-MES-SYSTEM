namespace MiniMES.Api.Dtos.Equipments;

public class UpdateEquipmentRequest
{
    public string EquipmentName { get; set; } = string.Empty;

    public int ProcessId { get; set; }

    public string Status { get; set; } = "Idle";

    public string? Location { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
