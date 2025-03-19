namespace TodoApp.DTOs
{
    public class DependencyCreateDto
    {
        public int DependentTaskId { get; set; }  // The task that depends on another task
        public int DependencyTaskId { get; set; }  // The task that must be completed first
    }

    public class DependencyResponseDto
    {
        public int Id { get; set; }
        public int DependentTaskId { get; set; }
        public int DependencyTaskId { get; set; }
        public string DependentTaskTitle { get; set; } = string.Empty;
        public string DependencyTaskTitle { get; set; } = string.Empty;
    }

    public class DependencyHierarchyDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<DependencyHierarchyDto> Dependencies { get; set; } = new List<DependencyHierarchyDto>();
        public int Level { get; set; } 
    }
}