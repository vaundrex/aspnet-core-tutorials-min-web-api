using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
// builder.Services.ConfigureHttpJsonOptions(options => {
//     options.SerializerOptions.WriteIndented = true;
//     // options.SerializerOptions.IncludeFields = true;
// });
var app = builder.Build();

var todoItems = app.MapGroup("/todoitems");

app.MapGet("/", () => "Welcome to the Todo API");

app.MapGet("/throw", () => new Exception("Sample exception.")); 

todoItems.MapGet("/", GetAllTodos);

todoItems.MapGet("/complete", GetCompleteTodos);

todoItems.MapGet("/{id}", async Task<Results<Ok<TodoItemDTO>, NotFound>> (int Id, TodoDb db) =>
    await db.Todos.FindAsync(Id)
        is Todo todo
            ? TypedResults.Ok(new TodoItemDTO(todo))
            : TypedResults.NotFound()
);

todoItems.MapPost("/", async Task<IResult> (TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todo = new Todo{
        Name = todoItemDTO.Name,
        IsComplete = todoItemDTO.IsComplete
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

todoItems.MapPut("/{id}", async Task<IResult> (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = todoItemDTO.Name;
    todo.IsComplete = todoItemDTO.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

todoItems.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
});

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Select(t => new TodoItemDTO(t)).ToArrayAsync());
}

static async Task<IResult> GetCompleteTodos(TodoDb db)
{
    return TypedResults.Ok(
        await db.Todos
        .Where(t => t.IsComplete)
        .Select(t => new TodoItemDTO(t)).ToArrayAsync());
}