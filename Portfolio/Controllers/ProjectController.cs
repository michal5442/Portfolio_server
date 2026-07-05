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
            // if (project is null)
            //     return BadRequest("Request body must not be null.");
            try
            {
                var updatedProject = await _repository.UpdateProject(project);
                if (updatedProject == null)
                    return NotFound(new { message = $"Failed to update project." });
                return Ok(updatedProject);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }     
        } 

        [HttpGet("getProjectById/{id}")]
        public async Task<ActionResult<Project>> GetProjectById([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Project ID must not be empty.");
 
            try
            {
                var project = await _repository.GetProjectById(id);
 
                if (project is null)
                    return NotFound($"Project with ID '{id}' was not found.");
 
                return Ok(project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
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
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }   

        [HttpGet("getProjectsByYear/{year}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsByYear([FromRoute] int year)
        {
            try
            {
                var projects = await _repository.GetProjectsByYear(year);

                if (projects is null)
                    return NotFound($"Projects for year '{year}' was not found.");

                return Ok(projects);
            }
            catch (Exception ex)
            {
                Console.WriteLine("======================= ERROR =======================");
    Console.WriteLine(ex.ToString());
    Console.WriteLine("=====================================================");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }       
        [HttpDelete("deleteProject/{id}")]
        public async Task<ActionResult<Project>> DeleteProject([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Project ID must not be empty.");

            try
            {
                var deleted = await _repository.DeleteProject(id);

                if (deleted is null)
                    return NotFound($"Project with ID '{id}' was not found.");

                return Ok(deleted);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }                                     
    }
}


