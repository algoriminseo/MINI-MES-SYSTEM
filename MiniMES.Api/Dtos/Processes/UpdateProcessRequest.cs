namespace MiniMES.Api.Dtos.Processes;

public class UpdateProcessRequest
{
    public string ProcessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsInspection { get; set; }

    public bool IsActive { get; set; } = true;
}
