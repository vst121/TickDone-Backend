namespace TickDone.Tests;

public class ToDoApiTests : IClassFixture<CustomWebApplicationFactory>, IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ToDoApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetAllToDos_EmptyAtStart_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/todos");
        response.EnsureSuccessStatusCode();

        var todos = await response.Content.ReadFromJsonAsync<List<ToDo>>();
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task CreateToDo_ValidRequest_Returns201AndLocationHeader()
    {
        var request = new CreateToDoRequest("This task name is definitely longer than 10 chars", new DateTime(2026, 12, 31));

        var response = await _client.PostAsJsonAsync("/todos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("/todos/", response.Headers.Location!.ToString());

        var createdTodo = await response.Content.ReadFromJsonAsync<ToDo>();
        Assert.NotNull(createdTodo);
        Assert.Equal(request.TaskName, createdTodo!.TaskName);
        Assert.Equal(request.Deadline, createdTodo.Deadline);
        Assert.False(createdTodo.Done);
    }

    [Fact]
    public async Task CreateToDo_ShortTaskName_Returns400WithCustomError()
    {
        var request = new CreateToDoRequest("short", DateTime.Today);

        var response = await _client.PostAsJsonAsync("/todos", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("TodoTooShort", problem.Extensions["error"]?.ToString());
        Assert.Contains("Todo must be at least 10 characters long.", problem.Extensions["message"]!.ToString());
    }

    [Fact]
    public async Task GetById_AfterCreate_ReturnsCorrectToDo()
    {
        var createResponse = await _client.PostAsJsonAsync("/todos", new CreateToDoRequest("Learn Minimal APIs testing", new DateTime(2025, 12, 25)));

        var created = await createResponse.Content.ReadFromJsonAsync<ToDo>();
        var id = created!.Id;

        var getResponse = await _client.GetAsync($"/todos/{id}");
        getResponse.EnsureSuccessStatusCode();

        var fetched = await getResponse.Content.ReadFromJsonAsync<ToDo>();
        Assert.Equal(id, fetched!.Id);
        Assert.Equal("Learn Minimal APIs testing", fetched.TaskName);
    }

    [Fact]
    public async Task GetById_NonExistent_Returns404()
    {
        var response = await _client.GetAsync("/todos/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateToDo_MarkAsDone_Returns200AndUpdatedEntity()
    {
        var createResp = await _client.PostAsJsonAsync("/todos", new CreateToDoRequest("Write integration tests", DateTime.Today.AddDays(7)));
        var todo = await createResp.Content.ReadFromJsonAsync<ToDo>();

        var updated = todo?.Done == true;
        var putResponse = await _client.PutAsJsonAsync($"/todos/{todo?.Id}", updated);

        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var result = await putResponse.Content.ReadFromJsonAsync<ToDo>();
        Assert.True(result!.Done);
    }

    [Fact]
    public async Task UpdateToDo_NonExistent_Returns404()
    {
        var fakeTodo = new ToDo { Id = 999, TaskName = "ghost", Done = true };
        var response = await _client.PutAsJsonAsync("/todos/999", fakeTodo);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteToDo_Existing_Returns204AndGoneAfterward()
    {
        var createResp = await _client.PostAsJsonAsync("/todos", new CreateToDoRequest("Temporary todo to delete", DateTime.Today));
        var todo = await createResp.Content.ReadFromJsonAsync<ToDo>();

        var deleteResponse = await _client.DeleteAsync($"/todos/{todo!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getAfterDelete = await _client.GetAsync($"/todos/{todo.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getAfterDelete.StatusCode);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }
}