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
    public class TsevetMevatseaController : ControllerBase
    {
        private readonly ITsevetMevatseaRepository _repository;
        private readonly ILogger<TsevetMevatseaController> _logger;

        public TsevetMevatseaController(ITsevetMevatseaRepository repository, ILogger<TsevetMevatseaController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost("insertTsevetMevatsea")]
        public async Task<ActionResult<TsevetMevatsea>> InsertTsevetMevatsea([FromBody] TsevetMevatsea tsevetMevatsea)
        {
            var insertedTeam = await _repository.InsertTsevetMevatsea(tsevetMevatsea);
            _logger.LogInformation("Inserted new tsevet mevatsea with IdntTsevetMevatsea: {TeamId}", insertedTeam.IdntTsevetMevatsea);
            return CreatedAtAction(nameof(GetTsevetMevatseaById), new { id = insertedTeam.IdntTsevetMevatsea }, insertedTeam);
        }

        [HttpGet("getAllTsevetMevatsea")]
        public async Task<ActionResult<IEnumerable<TsevetMevatsea>>> GetAllTsevetMevatsea()
        {
            var teams = await _repository.GetAllTsevetMevatsea();
            _logger.LogInformation("Retrieved {Count} tsevet mevatsea entries.", teams?.Count() ?? 0);
            return Ok(teams);
        }

        [HttpGet("getTsevetMevatseaById/{id:guid}")]
        public async Task<ActionResult<TsevetMevatsea>> GetTsevetMevatseaById([FromRoute] Guid id)
        {
            var team = await _repository.GetTsevetMevatseaById(id);
            if (team is null)
            {
                _logger.LogWarning("TsevetMevatsea not found. IdntTsevetMevatsea: {TeamId}", id);
                return NotFound($"TsevetMevatsea with id {id} was not found.");
            }
            return Ok(team);
        }

        [HttpPut("updateTsevetMevatsea/{id:guid}")]
        public async Task<ActionResult<TsevetMevatsea>> UpdateTsevetMevatsea([FromRoute] Guid id, [FromBody] TsevetMevatsea tsevetMevatsea)
        {
            if (tsevetMevatsea is null)
            {
                _logger.LogWarning("TsevetMevatsea update attempted with null data for id: {TeamId}", id);
                return BadRequest("TsevetMevatsea data is required.");
            }

            var updated = await _repository.UpdateTsevetMevatsea(id, tsevetMevatsea);
            if (updated is null)
            {
                _logger.LogWarning("Failed to update TsevetMevatsea. IdntTsevetMevatsea: {TeamId}", id);
                return NotFound($"TsevetMevatsea with id {id} was not found or update failed.");
            }

            _logger.LogInformation("TsevetMevatsea updated successfully. IdntTsevetMevatsea: {TeamId}", id);
            return Ok(updated);
        }

        [HttpPatch("toggleTsevetMevatseaActive/{id:guid}")]
        public async Task<ActionResult<TsevetMevatsea>> ToggleTsevetMevatseaActive([FromRoute] Guid id)
        {
            var team = await _repository.ToggleTsevetMevatseaActive(id);
            if (team is null)
            {
                _logger.LogWarning("Failed to toggle TsevetMevatsea active status. IdntTsevetMevatsea: {TeamId}", id);
                return NotFound($"TsevetMevatsea with id {id} was not found.");
            }

            _logger.LogInformation("TsevetMevatsea active status toggled to {Active}. IdntTsevetMevatsea: {TeamId}", team.Active, id);
            return Ok(team);
        }
    }
}