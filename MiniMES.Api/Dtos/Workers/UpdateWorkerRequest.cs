namespace MiniMES.Api.Dtos.Workers;

public class UpdateWorkerRequest
{
    public string WorkerName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? Role { get; set; }

    public string? ShiftGroup { get; set; }

    public bool IsActive { get; set; } = true;
}
