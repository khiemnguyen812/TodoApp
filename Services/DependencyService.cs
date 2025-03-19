using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TodoApp.Database;
using TodoApp.DTOs;
using TodoApp.Interfaces;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class DependencyService : IDependencyService
    {
        private readonly TodoDbContext _context;
        private readonly ILogger<DependencyService> _logger;

        public DependencyService(TodoDbContext context, ILogger<DependencyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DependencyResponseDto?> CreateDependencyAsync(DependencyCreateDto dependencyDto)
        {
            var dependentTask = await _context.Tasks.FindAsync(dependencyDto.DependentTaskId);
            var dependencyTask = await _context.Tasks.FindAsync(dependencyDto.DependencyTaskId);

            if (dependentTask == null || dependencyTask == null)
            {
                return null;
            }

            // Check if this would create a circular dependency
            if (await HasCircularDependencyAsync(dependencyDto.DependentTaskId, dependencyDto.DependencyTaskId))
            {
                throw new InvalidOperationException("This dependency would create a circular reference.");
            }

            // Check if this dependency already exists
            var existingDependency = await _context.TaskDependencies
                .FirstOrDefaultAsync(d => d.DependentTaskId == dependencyDto.DependentTaskId &&
                                     d.DependencyTaskId == dependencyDto.DependencyTaskId);

            if (existingDependency != null)
            {
                throw new InvalidOperationException("This dependency already exists.");
            }

            var dependency = new TaskDependency
            {
                DependentTaskId = dependencyDto.DependentTaskId,
                DependencyTaskId = dependencyDto.DependencyTaskId,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskDependencies.Add(dependency);
            await _context.SaveChangesAsync();

            return new DependencyResponseDto
            {
                Id = dependency.Id,
                DependentTaskId = dependency.DependentTaskId,
                DependencyTaskId = dependency.DependencyTaskId,
                DependentTaskTitle = dependentTask.Title,
                DependencyTaskTitle = dependencyTask.Title
            };
        }

        public async Task<bool> DeleteDependencyAsync(int id)
        {
            var dependency = await _context.TaskDependencies.FindAsync(id);
            if (dependency == null)
            {
                return false;
            }

            _context.TaskDependencies.Remove(dependency);
            await _context.SaveChangesAsync();


            return true;
        }

        public async Task<IEnumerable<DependencyResponseDto>> GetTaskDependenciesAsync(int taskId)
        {
            var cacheKey = $"dependencies_{taskId}";

            var dependencies = await _context.TaskDependencies
                .Include(d => d.DependentTask)
                .Include(d => d.DependencyTask)
                .Where(d => d.DependentTaskId == taskId || d.DependencyTaskId == taskId)
                .Select(d => new DependencyResponseDto
                {
                    Id = d.Id,
                    DependentTaskId = d.DependentTaskId,
                    DependencyTaskId = d.DependencyTaskId,
                    DependentTaskTitle = d.DependentTask.Title,
                    DependencyTaskTitle = d.DependencyTask.Title
                })
                .ToListAsync();


            return dependencies;
        }

        public async Task<DependencyHierarchyDto?> GetTaskDependencyHierarchyAsync(int taskId)
        {
            var cacheKey = $"dependency_hierarchy_{taskId}";

            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return null;
            }

            // Use HashSet to track visited tasks to avoid infinite recursion
            var visitedTasks = new HashSet<int>();

            var result = await BuildDependencyHierarchyAsync(taskId, 0, visitedTasks);

            return result;
        }

        private async Task<DependencyHierarchyDto> BuildDependencyHierarchyAsync(
            int taskId, int level, HashSet<int> visitedTasks)
        {
            if (visitedTasks.Contains(taskId))
            {
                return new DependencyHierarchyDto
                {
                    TaskId = taskId,
                    Title = "[Circular Reference]",
                    Level = level
                };
            }

            visitedTasks.Add(taskId);

            var task = await _context.Tasks.FindAsync(taskId);

            var result = new DependencyHierarchyDto
            {
                TaskId = taskId,
                Title = task?.Title ?? "Unknown Task",
                Level = level
            };

            var dependencies = await _context.TaskDependencies
                .Where(d => d.DependentTaskId == taskId)
                .Select(d => d.DependencyTaskId)
                .ToListAsync();

            foreach (var dependencyId in dependencies)
            {
                var dependencyHierarchy = await BuildDependencyHierarchyAsync(
                    dependencyId, level + 1, new HashSet<int>(visitedTasks));

                result.Dependencies.Add(dependencyHierarchy);
            }

            return result;
        }

        public async Task<bool> HasCircularDependencyAsync(int dependentTaskId, int dependencyTaskId)
        {
            if (dependentTaskId == dependencyTaskId)
            {
                return true;
            }

            var visitedTasks = new HashSet<int>();

            return await CheckCircularDependencyAsync(dependencyTaskId, dependentTaskId, visitedTasks);
        }

        private async Task<bool> CheckCircularDependencyAsync(int currentTaskId, int targetTaskId,
            HashSet<int> visitedTasks)
        {
            if (visitedTasks.Contains(currentTaskId))
            {
                return false;
            }

            visitedTasks.Add(currentTaskId);

            var dependencies = await _context.TaskDependencies
                .Where(d => d.DependentTaskId == currentTaskId)
                .Select(d => d.DependencyTaskId)
                .ToListAsync();

            if (dependencies.Contains(targetTaskId))
            {
                return true;
            }

            foreach (var dependencyId in dependencies)
            {
                if (await CheckCircularDependencyAsync(dependencyId, targetTaskId, visitedTasks))
                {
                    return true;
                }
            }

            return false;
        }
    }
}