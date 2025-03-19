using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TodoApp.Database;
using TodoApp.DTOs;
using TodoApp.Interfaces;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class TaskService : ITaskService
    {
        private readonly TodoDbContext _context;
        private readonly ILogger<TaskService> _logger;

        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);

        public TaskService(TodoDbContext context, ILogger<TaskService> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<PagedTasksResponseDto> GetTasksAsync(
            int pageIndex = 1,
            int pageSize = 10,
            string? searchTerm = null,
            Status? status = null,
            Priority? priority = null,
            DateTime? dueDateStart = null,
            DateTime? dueDateEnd = null)
        {
            var cacheKey = $"tasks_{pageIndex}_{pageSize}_{searchTerm}_{status}_{priority}_{dueDateStart}_{dueDateEnd}";

            if (_cache.TryGetValue(cacheKey, out PagedTasksResponseDto cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Returning cached result for key: {CacheKey}", cacheKey);
                return cachedResult;
            }

            IQueryable<TodoTask> query = _context.Tasks;

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => t.Title.Contains(searchTerm) ||
                                     (t.Description != null && t.Description.Contains(searchTerm)));
            }

            if (status.HasValue)
            {
                query = query.Where(t => t.Status == status.Value);
            }

            if (priority.HasValue)
            {
                query = query.Where(t => t.Priority == priority.Value);
            }

            if (dueDateStart.HasValue)
            {
                query = query.Where(t => t.DueDate >= dueDateStart.Value);
            }

            if (dueDateEnd.HasValue)
            {
                query = query.Where(t => t.DueDate <= dueDateEnd.Value);
            }

            var totalCount = await query.CountAsync();

            var tasks = await query
                .OrderBy(t => t.DueDate)
                .ThenByDescending(t => t.Priority)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TaskResponseDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();

            var result = new PagedTasksResponseDto
            {
                Tasks = tasks,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            _cache.Set(cacheKey, result, _cacheDuration);
            _logger.LogInformation("Cached result for key: {CacheKey}", cacheKey);

            return result;
        }

        public async Task<TaskDetailResponseDto?> GetTaskByIdAsync(int id)
        {
            var cacheKey = $"task_{id}";

            if (_cache.TryGetValue(cacheKey, out TaskDetailResponseDto cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Returning cached result for key: {CacheKey}", cacheKey);
                return cachedResult;
            }

            var task = await _context.Tasks
                .Include(t => t.DependencyOf)
                    .ThenInclude(d => d.DependencyTask)
                .Include(t => t.DependentOn)
                    .ThenInclude(d => d.DependentTask)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return null;
            }

            var taskDto = new TaskDetailResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Dependencies = task.DependencyOf.Select(d => new DependencyDto
                {
                    Id = d.DependencyTaskId,
                    Title = d.DependencyTask.Title,
                    Status = d.DependencyTask.Status
                }).ToList(),
                DependentTasks = task.DependentOn.Select(d => new DependencyDto
                {
                    Id = d.DependentTaskId,
                    Title = d.DependentTask.Title,
                    Status = d.DependentTask.Status
                }).ToList()
            };

            _cache.Set(cacheKey, taskDto, _cacheDuration);
            _logger.LogInformation("Cached result for key: {CacheKey}", cacheKey);

            return taskDto;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto taskDto)
        {
            var task = new TodoTask
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                Priority = taskDto.Priority,
                Status = Status.NotStarted,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            InvalidateTaskCache();

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int id, TaskUpdateDto taskDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return null;
            }

            // Update only provided properties
            if (taskDto.Title != null)
                task.Title = taskDto.Title;

            if (taskDto.Description != null) // Allow explicit null to clear description
                task.Description = taskDto.Description;

            if (taskDto.DueDate.HasValue)
                task.DueDate = taskDto.DueDate;

            if (taskDto.Priority.HasValue)
                task.Priority = taskDto.Priority.Value;

            if (taskDto.Status.HasValue)
                task.Status = taskDto.Status.Value;

            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            InvalidateTaskCache(id);

            return new TaskResponseDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return false;
            }

            // Remove dependencies first
            var dependencies = await _context.TaskDependencies
                .Where(td => td.DependentTaskId == id || td.DependencyTaskId == id)
                .ToListAsync();

            _context.TaskDependencies.RemoveRange(dependencies);
            _context.Tasks.Remove(task);

            await _context.SaveChangesAsync();

            InvalidateTaskCache(id);

            return true;
        }

        private void InvalidateTaskCache(int? taskId = null)
        {
            if (taskId.HasValue)
            {
                _cache.Remove($"task_{taskId.Value}");
                _logger.LogInformation("Cache invalidated for task_{TaskId}", taskId.Value);
            }

            _cache.Remove("tasks_marker");
        }
    }
}