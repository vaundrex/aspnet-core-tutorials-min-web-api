using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ToDoApi;

public static class TodoEndpoints
{
    
    public static RouteGroupBuilder MapTodoApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<IResult> (TodoDb db) => 
            TypedResults.Ok(await db.Todos.Select(t => new TodoItemDTO(t)).ToArrayAsync())
        );

        group.MapGet("/complete", async Task<IResult> (TodoDb db) => 
            TypedResults.Ok(
                await db.Todos
                .Where(t => t.IsComplete)
                .Select(t => new TodoItemDTO(t)).ToArrayAsync())
        );

        group.MapGet("/{id}", async Task<Results<Ok<TodoItemDTO>, NotFound>> (int Id, TodoDb db) =>
            await db.Todos.FindAsync(Id)
                is Todo todo
                    ? TypedResults.Ok(new TodoItemDTO(todo))
                    : TypedResults.NotFound()
        );

        group.MapPost("/", async Task<IResult> (TodoItemDTO todoItemDTO, TodoDb db) =>
        {
            var todo = new Todo{
                Name = todoItemDTO.Name,
                IsComplete = todoItemDTO.IsComplete
            };

            db.Todos.Add(todo);
            await db.SaveChangesAsync();

            return Results.Created($"/todoitems/{todo.Id}", todo);
        });

        group.MapPut("/{id}", async Task<IResult> (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
        {
            var todo = await db.Todos.FindAsync(id);
            if (todo is null) return Results.NotFound();

            todo.Name = todoItemDTO.Name;
            todo.IsComplete = todoItemDTO.IsComplete;

            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (int id, TodoDb db) =>
        {
            if (await db.Todos.FindAsync(id) is Todo todo)
            {
                db.Todos.Remove(todo);
                await db.SaveChangesAsync();
                return TypedResults.NoContent();
            }

            return TypedResults.NotFound();
        });

        return group;
    }
}