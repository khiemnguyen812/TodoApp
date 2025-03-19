using TodoApp.Models;

namespace TodoApp.DTOs
{
    public class TaskCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
    }

    public class TaskUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority? Priority { get; set; }
        public Status? Status { get; set; }
    }

    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TaskDetailResponseDto : TaskResponseDto
    {
        public List<DependencyDto> Dependencies { get; set; } = new List<DependencyDto>();
        public List<DependencyDto> DependentTasks { get; set; } = new List<DependencyDto>();
    }

    public class DependencyDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public Status Status { get; set; }
    }

    public class PagedTasksResponseDto
    {
        public List<TaskResponseDto> Tasks { get; set; } = new List<TaskResponseDto>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}