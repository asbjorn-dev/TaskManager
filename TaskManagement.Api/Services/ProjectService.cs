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
        public ProjectService(IProjectRepository repository)
        {
            _repository = repository;
        }


        public async Task<IEnumerable<ProjectResponseDto>> GetAllAsync()
        {
            var projects = await _repository.GetAllAsync();
            
            // maps model til DTO
            return projects.Select(p => new ProjectResponseDto
            {
                Id = p.Id,
                TeamId = p.TeamId,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<ProjectResponseDto?> GetByIdAsync(Guid id)
        {
            // find project udfra id parameter først
            var project = await _repository.GetByIdAsync(id);

            if (project == null)
                return null;
            
            // maps model til DTO
            return new ProjectResponseDto
            {
                Id = project.Id,
                TeamId = project.TeamId,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt
            };
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
            return await _repository.DeleteAsync(id);
        }
    }
}
