namespace WebAPI.Models;

public class TodoItem
{
    public long Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }

    public string OwnerId { get; set; } = null!;
    public int Priority { get; set; } // 0 = backlog, 1..3 priorities
}
