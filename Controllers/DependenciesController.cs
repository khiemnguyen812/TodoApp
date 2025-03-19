using Microsoft.AspNetCore.Mvc;
using TodoApp.DTOs;
using TodoApp.Interfaces;
using TodoApp.Services;

namespace TodoApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DependenciesController : ControllerBase
    {
        private readonly IDependencyService _dependencyService;

        public DependenciesController(IDependencyService dependencyService)
        {
            _dependencyService = dependencyService;
        }

        /// <summary>
        /// Gets dependencies for a specific task
        /// </summary>
        [HttpGet("task/{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DependencyResponseDto>>> GetTaskDependencies(int taskId)
        {
            var dependencies = await _dependencyService.GetTaskDependenciesAsync(taskId);
            return Ok(dependencies);
        }

        /// <summary>
        /// Gets the full dependency hierarchy for a task
        /// </summary>
        [HttpGet("hierarchy/{taskId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DependencyHierarchyDto>> GetTaskDependencyHierarchy(int taskId)
        {
            var hierarchy = await _dependencyService.GetTaskDependencyHierarchyAsync(taskId);
            if (hierarchy == null)
            {
                return NotFound();
            }
            return Ok(hierarchy);
        }

        /// <summary>
        /// Creates a new dependency between tasks
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DependencyResponseDto>> CreateDependency(DependencyCreateDto dependencyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var dependency = await _dependencyService.CreateDependencyAsync(dependencyDto);
                if (dependency == null)
                {
                    return NotFound("One or both tasks not found");
                }
                return CreatedAtAction(nameof(GetTaskDependencies), new { taskId = dependencyDto.DependentTaskId }, dependency);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Deletes a dependency by its ID
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDependency(int id)
        {
            var result = await _dependencyService.DeleteDependencyAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        /// <summary>
        /// Checks if adding a dependency would create a circular reference
        /// </summary>
        [HttpGet("check-circular")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<bool>> CheckCircularDependency(
            [FromQuery] int dependentTaskId,
            [FromQuery] int dependencyTaskId)
        {
            var result = await _dependencyService.HasCircularDependencyAsync(dependentTaskId, dependencyTaskId);
            return Ok(new { WouldCreateCircularDependency = result });
        }
    }
}