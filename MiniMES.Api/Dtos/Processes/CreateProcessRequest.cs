namespace MiniMES.Api.Dtos.Processes;

public class CreateProcessRequest
{
    public string ProcessCode { get; set; } = string.Empty;

    public string ProcessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsInspection { get; set; }
}
