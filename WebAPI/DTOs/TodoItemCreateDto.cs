using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs;

public class TodoItemCreateDto
{
    [Required]
    public string Name { get; set; } = null!;
    public bool IsComplete { get; set; }
    public int Priority { get; set; }
}

