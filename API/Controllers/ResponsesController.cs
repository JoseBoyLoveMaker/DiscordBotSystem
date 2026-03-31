using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/guilds/{guildId}/responses")]
    public class ResponsesController : ControllerBase
    {
        private readonly ResponseServiceAPI _responses;

        public ResponsesController(ResponseServiceAPI responses)
        {
            _responses = responses;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(ulong guildId)
        {
            var data = await _responses.GetAllResponses(guildId);
            return Ok(data);
        }

        [HttpGet("{trigger}")]
        public async Task<IActionResult> Get(ulong guildId, string trigger)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            var data = await _responses.GetByTrigger(guildId, decoded);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrigger(ulong guildId, [FromBody] string trigger)
        {
            if (string.IsNullOrWhiteSpace(trigger))
                return BadRequest("Trigger inválida.");

            await _responses.CreateTrigger(guildId, trigger.Trim());
            return Ok();
        }

        [HttpPost("{trigger}")]
        public async Task<IActionResult> AddResponse(ulong guildId, string trigger, [FromBody] string nova)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            if (string.IsNullOrWhiteSpace(nova))
                return BadRequest("Resposta inválida.");

            await _responses.AddResponse(guildId, decoded, nova);
            return Ok();
        }

        [HttpPut("{trigger}/{index:int}")]
        public async Task<IActionResult> EditResponse(ulong guildId, string trigger, int index, [FromBody] string nova)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            if (string.IsNullOrWhiteSpace(nova))
                return BadRequest("Resposta inválida.");

            await _responses.EditResponse(guildId, decoded, index, nova);
            return Ok();
        }

        [HttpDelete("{trigger}/{index:int}")]
        public async Task<IActionResult> DeleteResponse(ulong guildId, string trigger, int index)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            await _responses.DeleteResponse(guildId, decoded, index);
            return Ok();
        }

        [HttpDelete("{trigger}")]
        public async Task<IActionResult> DeleteTrigger(ulong guildId, string trigger)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            await _responses.DeleteTrigger(guildId, decoded);
            return Ok();
        }
    }
}