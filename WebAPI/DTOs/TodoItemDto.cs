namespace WebAPI.DTOs;

public class TodoItemDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsComplete { get; set; }
    public int Priority { get; set; }
}
