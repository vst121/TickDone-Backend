namespace TickDone.API.Requests;

public record CreateToDoRequest(string TaskName, DateTime? Deadline);