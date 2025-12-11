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

var todos = new[]
{
    "Task1 for learning React", "Task2 for learning German", "Task3 for going to gym", "Task4 for learning AI", "Task5 for washing dishes", "Task6 for cleaning the house", "Task7 for watching TV"
};

app.MapGet("/todos", () =>
{
    var todo = Enumerable.Range(1, 5).Select(index =>
        new ToDo
        (
            index.ToString(),
            todos[Random.Shared.Next(todos.Length)],
            DateTime.Now.AddDays(index),
            Random.Shared.Next(0, 1) == 1
        ))
        .ToArray();

    return todo;
})
.WithName("ToDos");

app.Run();

internal record ToDo(string Id, string Text, DateTime Deadline, bool Done)
{
    //public string Id => Guid.NewGuid().ToString();
}
