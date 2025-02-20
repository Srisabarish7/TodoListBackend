using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListBackend.Data;
using TodoListBackend.Models;
using TodoListBackend.Services;

namespace TodoListBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TodoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public TodoController(AppDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        // Create a new task for the logged-in user
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] TodoTask task)
        {
            try
            {
                var userId = _currentUserService.GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID is missing.");
                }

                if (task == null)
                {
                    return BadRequest("Task data is required.");
                }

                if (string.IsNullOrEmpty(task.Title) || string.IsNullOrEmpty(task.Priority))
                {
                    return BadRequest("Title and Priority are required fields.");
                }

                task.UserId = userId;
                _context.TodoTasks.Add(task);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }



        // Get tasks for the logged-in user only
        [HttpGet("get-tasks")]
        public async Task<IActionResult> GetTasks()
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID is missing.");
            }

            var tasks = await _context.TodoTasks
                                      .Where(t => t.UserId == userId)
                                      .ToListAsync();

            if (!tasks.Any())
            {
                return NotFound("No tasks found for this user.");
            }

            return Ok(tasks);
        }

        // Update task only if it belongs to the logged-in user
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TodoTask task)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID is missing.");
            }

            // Ensure the task belongs to the user
            var existingTask = await _context.TodoTasks
                                             .Where(t => t.Id == id && t.UserId == userId)
                                             .FirstOrDefaultAsync();

            if (existingTask == null)
            {
                return NotFound("Task not found or you don't have permission to edit it.");
            }

            // Update only allowed fields
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.IsCompleted = task.IsCompleted;
            existingTask.DueDate = task.DueDate;
            existingTask.ReminderDate = task.ReminderDate;
            existingTask.Priority = task.Priority;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Delete task only if it belongs to the logged-in user
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID is missing.");
            }

            // Ensure the task belongs to the user
            var task = await _context.TodoTasks
                                     .Where(t => t.Id == id && t.UserId == userId)
                                     .FirstOrDefaultAsync();

            if (task == null)
            {
                return NotFound("Task not found or you don't have permission to delete it.");
            }

            _context.TodoTasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
