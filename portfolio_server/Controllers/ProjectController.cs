using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using portfolio_server.Models;
using portfolio_server.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace portfolio_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _repository;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IProjectRepository repository, ILogger<ProjectController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("insertProject")]
        public async Task<ActionResult<Project>> InsertProject([FromBody] Project project)
        {
            if (project is null)
            {
                _logger.LogWarning("Null request body received.");
                return BadRequest("Request body must not be null.");
            }
            var created = await _repository.InsertProject(project);
            _logger.LogInformation("Project created. ProjectId: {ProjectId}. PerformedBy: {PerformedBy}", created.Id, GetAuditUserIdentity());
            return Ok(created);
        }

        [HttpPut("updateProject")]
         public async Task<ActionResult<Project>> UpdateProject([FromBody] Project project)
        {
            if (project is null)
            {
                _logger.LogWarning("Null request body received.");
                return BadRequest("Request body must not be null.");
            }

            var updatedProject = await _repository.UpdateProject(project);

            if (updatedProject is null)
            {
                _logger.LogWarning("Project not found. ProjectId: {ProjectId}", project.Id);
                return NotFound($"Project with ID {project.Id} was not found.");
            }

            _logger.LogInformation("Project updated. ProjectId: {ProjectId}. PerformedBy: {PerformedBy}", project.Id, GetAuditUserIdentity());
            return Ok(updatedProject);
        }

        [HttpGet("getProjectById/{id:guid}")]
        public async Task<ActionResult<Project>> GetProjectById([FromRoute] Guid id)
        {
            var project = await _repository.GetProjectById(id);

            if (project is null)
            {
                _logger.LogWarning("Project not found. ProjectId: {ProjectId}", id);
                return NotFound($"Project with ID '{id}' was not found.");
            }

            _logger.LogInformation("Project retrieved. ProjectId: {ProjectId}", id);
            return Ok(project);
        }

        [HttpGet("getProjectsByYear/{year}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByYear([FromRoute] int year)
        {
            var projects = await _repository.GetProjectsByYear(year);
            _logger.LogInformation("Retrieved {Count} projects for year {Year}.", projects?.Count() ?? 0, year);
            return Ok(projects);
        }

        [HttpGet("getAllProjects")]
        public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
        {
            var projects = await _repository.GetAllProjects();
            _logger.LogInformation("Retrieved {Count} active projects.", projects?.Count() ?? 0);
            return Ok(projects);
        }

        [HttpPost("copyProjectsFromPreviousYear/{year}")]
        public async Task<ActionResult<IEnumerable<Project>>> CopyProjectsFromPreviousYear([FromRoute] int year)
        {
            if (year <= 1)
            {
                _logger.LogWarning("Invalid year provided for copy operation. Year: {Year}", year);
                return BadRequest("A valid year must be provided.");
            }
 
            var copiedProjects = (await _repository.CopyProjectsFromPreviousYear(year)).ToList();
 
            if (copiedProjects.Count == 0)
            {
                _logger.LogInformation("No projects found for year {PreviousYear} to copy into {Year}.", year - 1, year);
                return NotFound($"No projects were found for year {year - 1} to copy.");
            }
 
            _logger.LogInformation(
                "Copied {Count} projects from year {PreviousYear} to year {Year}. PerformedBy: {PerformedBy}",
                copiedProjects.Count, year - 1, year, GetAuditUserIdentity());
 
            return Ok(copiedProjects);
        }
      
        [HttpDelete("deleteProject/{id:guid}")]
        public async Task<ActionResult<Project>> DeleteProject([FromRoute] Guid id)
        {
            var deleted = await _repository.DeleteProject(id);

            if (deleted is null)
            {
                _logger.LogWarning("Project not found. ProjectId: {ProjectId}", id);
                return NotFound($"Project with ID '{id}' was not found.");
            }

            _logger.LogInformation("Project deleted. ProjectId: {ProjectId}. PerformedBy: {PerformedBy}", id, GetAuditUserIdentity());
            return Ok(deleted);
        }

        [HttpPatch("toggleProjectActive/{id:guid}")]
        public async Task<ActionResult<Project>> ToggleProjectActive([FromRoute] Guid id)
        {
            var project = await _repository.ToggleProjectActive(id);

            if (project is null)
            {
                _logger.LogWarning("Project not found. ProjectId: {ProjectId}", id);
                return NotFound($"Project with ID '{id}' was not found.");
            }

            _logger.LogInformation("Project active status toggled to {Active}. ProjectId: {ProjectId}. PerformedBy: {PerformedBy}", project.Active, id, GetAuditUserIdentity());
            return Ok(project);
        }

        private string GetAuditUserIdentity()
        {
            var user = HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.Identity?.Name ?? user.FindFirst("sub")?.Value ?? "authenticated-user";
            }

            return "anonymous";
        }
    }
}