using TaskManagement.Api.Data;
using TaskManagement.Api.Dtos.Projects;
using TaskManagement.Api.Interfaces;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Services
{
    // service layer indeholder kun business logik (validering, map DTO <-> model)
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly ICacheService _cacheService;
        public ProjectService(IProjectRepository repository, ICacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }


        public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync()
        {
            // hent projects fra redis og return cache data hvis cache hit
            var cachedProjects = await _cacheService.GetAsync<IEnumerable<ProjectResponseDto>>("projects:all");
            if (cachedProjects != null)
                return cachedProjects;

            // cache miss - hent fra db
            var projects = await _repository.GetAllAsync();
            
            // maps model til DTO
            var result = projects.Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                TeamId = p.TeamId,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            }).ToList();

            // cache alle projects til redis med key
            await _cacheService.SetAsync("projects:all", result);

            return result;
        }

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id)
        {
            var cacheKey = $"projects:{id}"; // unik cache key f.eks. "project:123e456..."
            var cachedProject = await _cacheService.GetAsync<ProjectResponseDto>(cacheKey);
            if (cachedProject != null)
                return cachedProject;

            // find project udfra id parameter først
            var project = await _repository.GetByIdAsync(id);
            if (project == null)
                return null;
            
            // maps model til DTO
            var result = new ProjectResponseDto
            {
                Id = project.Id,
                TeamId = project.TeamId,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt
            };

            // cache project i redis
            await _cacheService.SetAsync(cacheKey, result);

            return result;
        }

        public async Task<ProjectResponseDto> CreateAsync(CreateProjectDto dto)
        {
            // valider først om team exist
            var teamExist = await _repository.TeamExistsAsync(dto.TeamId);
            
            if (!teamExist)
                throw new Exception($"Team with ID: {dto.TeamId} does not exist");
            
            // map DTO til model
            var project = new Project
            {
                Id = Guid.NewGuid(),
                TeamId = dto.TeamId,
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
            
            var createdProject = await _repository.AddAsync(project);
            
            // invalidate cache - slet alle cached projects når vi laver ny project
            await _cacheService.RemoveAsync("projects:all");

            // map model til DTO
            return new ProjectResponseDto
            {
                Id = createdProject.Id,
                TeamId = createdProject.TeamId,
                Name = createdProject.Name,
                Description = createdProject.Description,
                Status = createdProject.Status,
                CreatedAt = createdProject.CreatedAt
            };
        }

        public async Task<ProjectResponseDto?> UpdateAsync(Guid id, UpdateProjectDto dto)
        {
            var projectToUpdate = await _repository.GetByIdAsync(id);

            if (projectToUpdate == null)
                return null;
            
            // updater properties
            projectToUpdate.Name = dto.Name;
            projectToUpdate.Description = dto.Description;
            projectToUpdate.Status = dto.Status;
            
            var updatedProject = await _repository.UpdateAsync(projectToUpdate);
            
            // cache invalidation - slet alle projects og det specifikke project fra cache så GetByIdAsync ikke har gammel cache
            await _cacheService.RemoveAsync("projects:all");
            await _cacheService.RemoveAsync($"projects:{id}");

            // map model til DTO
            return new ProjectResponseDto
            {
                Id = updatedProject.Id,
                TeamId = updatedProject.TeamId,
                Name = updatedProject.Name,
                Description = updatedProject.Description,
                Status = updatedProject.Status,
                CreatedAt = updatedProject.CreatedAt
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var ProjectToDelete = await _repository.DeleteAsync(id);

            if (ProjectToDelete)
            {
                await _cacheService.RemoveAsync("projects:all");
                await _cacheService.RemoveAsync($"projects:{id}");
            }
            
            return ProjectToDelete;
        }
    }
}
