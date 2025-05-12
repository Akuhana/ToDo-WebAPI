using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using WebAPI.DTOs;
using WebAPI.Data;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
        private readonly IHttpContextAccessor _httpContext;

        public TodoItemsController(TodoContext context, IHttpContextAccessor httpContext)
        {
            _context = context;
            _httpContext = httpContext;
        }

        // Helper to get current user’s Id from the JWT
        private string CurrentUserId =>
            _httpContext.HttpContext!.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? _httpContext.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User id claim missing");


        // GET: api/TodoItems
        // only return THIS user’s items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodoItems()
        {
            var userId = CurrentUserId;
            var dtos = await _context.TodoItems
                          .Where(t => t.OwnerId == userId) // Filter by owner
                          .Select(t => new TodoItemDto
                          {
                              Id = t.Id,
                              Name = t.Name!,
                              IsComplete = t.IsComplete,
                              Priority = t.Priority
                          })
                          .ToListAsync();
            return Ok(dtos);
        }

        // GET: api/TodoItems/5

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItemDto>> GetTodoItem(long id)
        {
            var t = await _context.TodoItems.FindAsync(id);
            if (t == null || t.OwnerId != CurrentUserId) // Verify ownership
                return NotFound();
            return new TodoItemDto
            {
                Id = t.Id,
                Name = t.Name!,
                IsComplete = t.IsComplete,
                Priority = t.Priority
            };
        }

        // POST: api/TodoItems
        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem([FromBody] TodoItemCreateDto dto)
        {
            var todoItem = new TodoItem
            {
                Name = dto.Name,
                IsComplete = dto.IsComplete,
                Priority = 0,                // always start in backlog
                OwnerId = CurrentUserId    // injected from JWT
            };

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem),
                                   new { id = todoItem.Id },
                                   todoItem);
        }

        // PUT: api/TodoItems/5 - you can use this to change name, isComplete or priority
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(long id, [FromBody] TodoItemUpdateDto dto)
        {
            var existing = await _context.TodoItems.FindAsync(id);
            if (existing == null || existing.OwnerId != CurrentUserId) // Verify ownership
                return NotFound();

            existing.Name = dto.Name;
            existing.IsComplete = dto.IsComplete;
            existing.Priority = dto.Priority;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/TodoItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var t = await _context.TodoItems.FindAsync(id);
            if (t == null || t.OwnerId != CurrentUserId) // Verify ownership
                return NotFound();

            _context.TodoItems.Remove(t);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }
    }
}
