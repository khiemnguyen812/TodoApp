using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp.Models
{
    public class TaskDependency
    {
        public int Id { get; set; }

        [Required]
        public int DependentTaskId { get; set; } 

        [Required]
        public int DependencyTaskId { get; set; }  

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DependentTaskId")]
        public virtual TodoTask DependentTask { get; set; } = null!;

        [ForeignKey("DependencyTaskId")]
        public virtual TodoTask DependencyTask { get; set; } = null!;
    }
}