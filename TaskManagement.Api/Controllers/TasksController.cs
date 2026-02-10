using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Dtos.Tasks;
using TaskManagement.Api.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }


        // GET: api/Tasks - Get all tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetAllTasks()
        {
            var tasks = await _taskService.GetAllAsync();

            return Ok(tasks); // return [] hvis tom
        }

        // GET: api/Tasks/{id} 
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskResponseDto>> GetById(Guid id)
        {
            var tasks = await _taskService.GetByIdAsync(id);

            return tasks != null ? Ok(tasks) : NotFound($"Task with ID: {id} is not found");
        }

        // GET: api/Tasks/project/{projectId}
        [HttpGet("project/{projectId:guid}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetByProjectId(Guid projectId)
        {
            var tasks = await _taskService.GetByProjectIdAsync(projectId);

            return tasks != null ? Ok(tasks) : NotFound($"Task not found with project ID: {projectId}");
        }

        // POST: api/Tasks 
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> Create([FromBody] CreateTaskDto dto)
        {
            try
            {
                var created = await _taskService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // UPDATE: api/tasks/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TaskResponseDto>> Update(Guid id, [FromBody] UpdateTaskDto dto)
        {
            // try/catch - null hvis task med id ikke findes og catch hvis task findes, men userId er ugyldig
            try
            {
                var updatedTask = await _taskService.UpdateAsync(id, dto);

                return updatedTask != null ? Ok(updatedTask) : NotFound($"Task with ID: {id} is not found");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // TODO: overvej at tilføje PATCH endpoint til håndtere kun opdater tags

        // DELETE: api/tasks/{id}
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deletedTask = await _taskService.DeleteAsync(id);

            return deletedTask ? NoContent() : NotFound($"Task with ID: {id} is not found");
        }
    }
}
