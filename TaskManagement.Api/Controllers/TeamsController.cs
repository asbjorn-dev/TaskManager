using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.Dtos.Teams;
using TaskManagement.Api.Interfaces;

namespace TaskManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;
        public TeamsController(ITeamService teamService)
        {
            _teamService = teamService;
        }
        
        // GET: api/teams
        [HttpGet]
        public async Task<ActionResult<TeamResponseDto>> GetAll()
        {
            var teams = await _teamService.GetAllAsync();

            return Ok(teams);
        }

        // GET: api/teams/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TeamResponseDto>> GetById(Guid id)
        {
            var team = await _teamService.GetByIdAsync(id);

            return team != null ? Ok(team) : NotFound($"Team with ID: {id} is not found");
        }

        // POST: api/teams
        [HttpPost]
        public async Task<ActionResult<TeamResponseDto>> Create([FromBody] CreateTeamDto dto)
        {
            var createdTeam = await _teamService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new {id = createdTeam.Id}, createdTeam);
        }

        // PUT: api/teams/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<TeamResponseDto>> Update(Guid id, [FromBody] UpdateTeamDto dto)
        {
          var updated = await _teamService.UpdateAsync(id, dto);
          return updated != null ? Ok(updated) : NotFound($"Team with ID: {id} not found.");
        }

        // DELETE: api/teams/{id}
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var deletedTeam = await _teamService.DeleteAsync(id);

            return deletedTeam ? NoContent() : NotFound($"Team wth ID: {id} is not found");
        }
    }
}
