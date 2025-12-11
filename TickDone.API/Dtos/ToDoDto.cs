namespace TickDone.API.Dtos;

public record CreateToDoRequest(string TaskName, DateTime? Deadline);