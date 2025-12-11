namespace TickDone.API.Endpoints;

public static class ToDoEndpoints
{
    public static RouteGroupBuilder MapToDoEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var todos = await db.ToDos.ToListAsync();
            return Results.Ok(todos);
        })
.WithName("GetToDos");

        group.MapGet("/{id}", async (int id, AppDbContext db) =>
        {
            var todo = await db.ToDos.FindAsync(id);

            if (todo is null)
                return Results.NotFound();

            return Results.Ok(todo);
        })
        .WithName("GetToDoById");

        group.MapPost("/", async (CreateToDoRequest request, AppDbContext db) =>
        {
            if (request.TaskName.Trim().Length < 10)
                return Results.BadRequest(new
                {
                    error = "TaskNameTooShort",
                    message = "Task must be at least 10 characters long.",
                    field = "TaskName"
                });

            var todo = new ToDo
            {
                TaskName = request.TaskName,
                Deadline = request.Deadline,
            };

            db.ToDos.Add(todo);
            await db.SaveChangesAsync();

            return Results.Created($"/todos/{todo.Id}", todo);
        })
        .WithName("CreateToDo");

        group.MapPut("/{id}", async (int id, ToDo updatedTodo, AppDbContext db) =>
        {
            var todo = await db.ToDos.FindAsync(id);

            if (todo is null)
                return Results.NotFound();

            todo.Done = updatedTodo.Done;

            await db.SaveChangesAsync();

            return Results.Ok(todo);
        })
        .WithName("UpdateTodo");

        group.MapDelete("/{id}", async (int id, AppDbContext db) =>
        {
            var todo = await db.ToDos.FindAsync(id);

            if (todo is null)
                return Results.NotFound();

            db.ToDos.Remove(todo);
            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteToDo");

        return group;
    }
}