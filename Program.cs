using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

var todoItems = app.MapGroup("/todoitems");

app.MapGet("/", () => "Welcome to the Todo API");

todoItems.MapGet("/", async (TodoDb db) => 
    await db.Todos.ToListAsync());

todoItems.MapGet("/complete", async (TodoDb db) => 
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

todoItems.MapGet("/{id}", async Task<Results<Ok<Todo>, NotFound>> (int Id, TodoDb db) =>
    await db.Todos.FindAsync(Id)
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound()
);

todoItems.MapPost("/", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

todoItems.MapPut("/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

todoItems.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (int id, TodoDb db) => 
{
    if(await db.Todos.FindAsync(id) is Todo todo){
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
});

app.Run();
