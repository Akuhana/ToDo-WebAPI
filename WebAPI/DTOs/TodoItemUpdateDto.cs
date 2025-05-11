using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

public class TodoItemUpdateDto
{
    [Required]
    public string Name { get; set; } = null!;
    public bool IsComplete { get; set; }
    public int Priority { get; set; }
}