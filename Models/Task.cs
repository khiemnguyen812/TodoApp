using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models
{
    public class TodoTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime? DueDate { get; set; }

        [Required]
        public Priority Priority { get; set; } = Priority.Medium;
        [Required]
        public Status Status { get; set; } = Status.NotStarted;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<TaskDependency> DependencyOf { get; set; } = new List<TaskDependency>();
        public virtual ICollection<TaskDependency> DependentOn { get; set; } = new List<TaskDependency>();
    }
    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum Status
    {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2,
        Blocked = 3,
        Cancelled = 4
    }
}