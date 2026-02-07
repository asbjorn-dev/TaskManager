using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Dtos.Projects;
using TaskManagement.Api.Interfaces;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    
    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }
    
    
    // GET: api/projects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetAll()
    {
        var projects = await _projectService.GetAllAsync();

        return Ok(projects); // return [] hvis tom
    }
    
    // GET: api/projects/{id}
    // {id:guid} siger til asp.net at id skal være et guid
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectResponseDto>> GetById(Guid id)
    {
        var projectToFind = await _projectService.GetByIdAsync(id);
        
        return projectToFind != null ? Ok(projectToFind) : NotFound($"Project with ID: {id} not found.");
    }
    
    // POST: api/projects
    [HttpPost]
    public async Task<ActionResult<ProjectResponseDto>> Create([FromBody] CreateProjectDto dto)
    {
        try
        {
            var created = await _projectService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    // PUT: api/projects/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponseDto>> Update(Guid id, [FromBody] UpdateProjectDto dto)
    {
        var updatedProject = await _projectService.UpdateAsync(id, dto);
        
        return updatedProject != null ? Ok(updatedProject) : NotFound($"Project with ID: {id} not found.");
    }
    
    // DELETE: api/projects/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deletedProject = await _projectService.DeleteAsync(id);
        
        return deletedProject ? Ok() : NotFound($"Project with ID: {id} not found.");
    }
}