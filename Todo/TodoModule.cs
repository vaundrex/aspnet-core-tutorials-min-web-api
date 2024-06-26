﻿using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ToDoApi;

public static class TodoEndpoints
{
    public static RouteGroupBuilder AddTodoEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async Task<Ok<TodoItemDTO[]>> (TodoDb db) => 
            TypedResults.Ok(await db.Todos.Select(t => new TodoItemDTO(t)).ToArrayAsync())
        );

        group.MapGet("/complete", async Task<Ok<TodoItemDTO[]>> (TodoDb db) => 
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

        /// <summary>
        /// Creates a TodoItem.
        /// </summary>
        /// <response code="201">Returns the newly created item</response>
        /// <response code="400">If the item is null</response> 
        group.MapPost("/", async Task<Created<Todo>> (TodoItemDTO todoItemDTO, TodoDb db) =>
        {
            var todo = new Todo{
                Name = todoItemDTO.Name,
                IsComplete = todoItemDTO.IsComplete
            };

            db.Todos.Add(todo);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/todoitems/{todo.Id}", todo);
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "This is a summary",
            Description = "This is a description"
        });

        group.MapPut("/{id}", async Task<Results<NotFound, NoContent>> (int id, TodoItemDTO todoItemDTO, TodoDb db) =>
        {
            var todo = await db.Todos.FindAsync(id);
            if (todo is null) return TypedResults.NotFound();

            todo.Name = todoItemDTO.Name;
            todo.IsComplete = todoItemDTO.IsComplete;

            await db.SaveChangesAsync();

            return TypedResults.NoContent();
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