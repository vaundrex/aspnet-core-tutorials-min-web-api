using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ToDoApi;

public class TodoItemDTO
{
    public int Id {get; set; }

    public string? Name { get; set; }
    [DefaultValue(false)]    
    public bool IsComplete { get; set; }
    public TodoItemDTO(){}

    public TodoItemDTO(Todo todoItem) =>
        (Id, Name, IsComplete) = (todoItem.Id, todoItem.Name, todoItem.IsComplete);
}
