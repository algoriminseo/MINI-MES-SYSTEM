namespace MiniMES.Api.Dtos.Equipments;

public class CreateEquipmentRequest
{
    public string EquipmentCode { get; set; } = string.Empty;

    public string EquipmentName { get; set; } = string.Empty;

    public int ProcessId { get; set; }

    public string Status { get; set; } = "Idle";

    public string? Location { get; set; }

    public string? Description { get; set; }
}
