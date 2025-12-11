var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/todos", async (AppDbContext db) =>
{
    var todos = await db.ToDos.ToListAsync();
    return Results.Ok(todos);
})
.WithName("GetToDos");

app.MapGet("/todos/{id}", async (int id, AppDbContext db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null)
        return Results.NotFound();

    return Results.Ok(todo);
})
.WithName("GetToDoById");

app.MapPost("/todos", async (CreateToDoRequest request, AppDbContext db) =>
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

app.MapPut("/todos/{id}", async (int id, ToDo updatedTodo, AppDbContext db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null)
        return Results.NotFound();

    todo.Done = updatedTodo.Done;

    await db.SaveChangesAsync();

    return Results.Ok(todo);
})
.WithName("UpdateTodo");

app.MapDelete("/todos/{id}", async (int id, AppDbContext db) =>
{
    var todo = await db.ToDos.FindAsync(id);

    if (todo is null)
        return Results.NotFound();

    db.ToDos.Remove(todo);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteToDo");


app.Run();
