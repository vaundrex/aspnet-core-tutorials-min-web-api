using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/todoitems", async (TodoDb db) => 
    await db.Todos.ToListAsync());

app.MapGet("/todoitems/complete", async (TodoDb db) => 
    await db.Todos.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/todolist/{id}", async Results<Ok<Todo>, NotFound> (int Id, TodoDb db) =>
    await db.Todos.FindAsync(Id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound()
);

app.Run();
