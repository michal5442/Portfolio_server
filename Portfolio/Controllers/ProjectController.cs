using Microsoft.AspNetCore.Mvc;
using Portfolio.Models;
using Portfolio.Repositories;
using System.Threading.Tasks;

namespace Portfolio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectRepository _repository;

        public ProjectController(IProjectRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("insertProject")]
        public async Task<ActionResult<Project>> InsertProject([FromBody] Project project)
        {
             
            if(project is null)
                return BadRequest("Request body must not be null.");
            try
            {
                var created = await _repository.InsertProject(project);
                return Ok(created);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to create project.");
            }
        }

        [HttpPut("updateProject")]
         public async Task<ActionResult<Project>> UpdateProject([FromBody] Project project)
        {    
            if (project is null)
                return BadRequest("Request body must not be null.");
            try
            {
                var updatedProject = await _repository.UpdateProject(project);
                return Ok(updatedProject);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to update project.");
            }    
        } 

        [HttpGet("getAllProjects")]
        public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
        {
            try
            {
                var projects = await _repository.GetAllProjects();
                return Ok(projects);
            }
            catch (Exception)
            {
                return StatusCode(500, "Failed to retrieve projects.");
            }
        }   

        [HttpGet("getProjectById/{id}")]
        public async Task<ActionResult<Project>> GetProjectById([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Project ID must not be empty.");

            if (!Guid.TryParse(id, out _))
                return BadRequest("Invalid project ID format.");
 
            try
            {
                var project = await _repository.GetProjectById(id);
 
                if (project is null)
                    return NotFound($"Project with ID '{id}' was not found.");
 
                return Ok(project);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        } 

        [HttpGet("getProjectsByYear/{year}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByYear([FromRoute] int year)
        {
            try
            {
                var projects = await _repository.GetProjectsByYear(year);
                return Ok(projects);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        } 
      
        [HttpDelete("deleteProject/{id}")]
        public async Task<ActionResult<Project>> DeleteProject([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Project ID must not be empty.");

            if (!Guid.TryParse(id, out _))
                return BadRequest("Invalid project ID format.");

            try
            {
                var deleted = await _repository.DeleteProject(id);

                if (deleted is null)
                    return NotFound($"Project with ID '{id}' was not found.");

                return Ok(deleted);
            }
            catch (Exception)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }                                     
    }
}


