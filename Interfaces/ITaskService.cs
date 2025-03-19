using TodoApp.DTOs;
using TodoApp.Models;

namespace TodoApp.Interfaces
{
    public interface ITaskService
    {
        Task<PagedTasksResponseDto> GetTasksAsync(int pageIndex = 1, int pageSize = 10, string? searchTerm = null, 
            Status? status = null, Priority? priority = null, DateTime? dueDateStart = null, DateTime? dueDateEnd = null);
        
        Task<TaskDetailResponseDto?> GetTaskByIdAsync(int id);
        Task<TaskResponseDto> CreateTaskAsync(TaskCreateDto taskDto);
        Task<TaskResponseDto?> UpdateTaskAsync(int id, TaskUpdateDto taskDto);
        Task<bool> DeleteTaskAsync(int id);
    }
}