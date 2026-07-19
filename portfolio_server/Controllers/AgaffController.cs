using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using portfolio_server.Models;
using portfolio_server.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace portfolio_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgaffController : ControllerBase
    {
        private readonly IAgaffRepository _repository;
        private readonly ILogger<AgaffController> _logger;

        public AgaffController(IAgaffRepository repository, ILogger<AgaffController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("insertAgaff")]
        public async Task<ActionResult<Agaff>> InsertAgaff([FromBody] Agaff agaff)
        {
            var insertedAgaff = await _repository.InsertAgaff(agaff);
            _logger.LogInformation("Inserted new agaff with IdntAgaff: {AgaffId}", insertedAgaff.IdntAgaff);
            return CreatedAtAction(nameof(GetAgaffById), new { id = insertedAgaff.IdntAgaff }, insertedAgaff);
        }

        [HttpGet("getAllAgaff")]
        public async Task<ActionResult<IEnumerable<Agaff>>> GetAllAgaff()
        {
            var agaffList = await _repository.GetAllAgaff();
            _logger.LogInformation("Retrieved {Count} agaff entries.", agaffList?.Count() ?? 0);
            return Ok(agaffList);
        }

        [HttpGet("getAgaffById/{id:guid}")]
        public async Task<ActionResult<Agaff>> GetAgaffById([FromRoute] Guid id)
        {
            var agaff = await _repository.GetAgaffById(id);
            if (agaff is null)
            {
                _logger.LogWarning("Agaff not found. IdntAgaff: {AgaffId}", id);
                return NotFound($"Agaff with id {id} was not found.");
            }
            return Ok(agaff);
        }

        [HttpPut("updateAgaff/{id:guid}")]
        public async Task<ActionResult<Agaff>> UpdateAgaff([FromRoute] Guid id, [FromBody] Agaff agaff)
        {
            if (agaff is null)
            {
                _logger.LogWarning("Agaff update attempted with null data for id: {AgaffId}", id);
                return BadRequest("Agaff data is required.");
            }

            var updated = await _repository.UpdateAgaff(id, agaff);
            if (updated is null)
            {
                _logger.LogWarning("Failed to update agaff. IdntAgaff: {AgaffId}", id);
                return NotFound($"Agaff with id {id} was not found or update failed.");
            }

            _logger.LogInformation("Agaff updated successfully. IdntAgaff: {AgaffId}", id);
            return Ok(updated);
        }

        [HttpPatch("toggleAgaffActive/{id:guid}")]
        public async Task<ActionResult<Agaff>> ToggleAgaffActive([FromRoute] Guid id)
        {
            var agaff = await _repository.ToggleAgaffActive(id);
            if (agaff is null)
            {
                _logger.LogWarning("Failed to toggle agaff active status. IdntAgaff: {AgaffId}", id);
                return NotFound($"Agaff with id {id} was not found.");
            }

            _logger.LogInformation("Agaff active status toggled to {Active}. IdntAgaff: {AgaffId}", agaff.Active, id);
            return Ok(agaff);
        }

    }
}