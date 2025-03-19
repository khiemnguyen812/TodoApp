using TodoApp.DTOs;

namespace TodoApp.Interfaces
{
    public interface IDependencyService
    {
        Task<DependencyResponseDto?> CreateDependencyAsync(DependencyCreateDto dependencyDto);
        Task<bool> DeleteDependencyAsync(int id);
        Task<IEnumerable<DependencyResponseDto>> GetTaskDependenciesAsync(int taskId);
        Task<DependencyHierarchyDto?> GetTaskDependencyHierarchyAsync(int taskId);
        Task<bool> HasCircularDependencyAsync(int dependentTaskId, int dependencyTaskId);
    }
}