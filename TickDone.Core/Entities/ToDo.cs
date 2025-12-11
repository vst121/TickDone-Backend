namespace TickDone.Core.Entities;

public class ToDo
{
    public int Id { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
    public bool Done { get; set; }
}