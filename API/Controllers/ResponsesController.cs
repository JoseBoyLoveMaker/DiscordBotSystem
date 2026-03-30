using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/responses")]
    public class ResponsesController : ControllerBase
    {
        private readonly ResponseServiceAPI _responses;

        public ResponsesController(ResponseServiceAPI responses)
        {
            _responses = responses;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _responses.GetAllResponses();
            return Ok(data);
        }

        [HttpGet("{trigger}")]
        public async Task<IActionResult> Get(string trigger)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            var data = await _responses.GetByTrigger(decoded);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost("{trigger}")]
        public async Task<IActionResult> Add(string trigger, [FromBody] string nova)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            await _responses.AddResponse(decoded, nova);

            return Ok();
        }

        [HttpDelete("{trigger}/{index}")]
        public async Task<IActionResult> Delete(string trigger, int index)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            await _responses.DeleteResponse(decoded, index);

            return Ok();
        }

        [HttpPut("{trigger}/{index}")]
        public async Task<IActionResult> Edit(string trigger, int index, [FromBody] string nova)
        {
            var decoded = Uri.UnescapeDataString(trigger);

            await _responses.EditResponse(decoded, index, nova);

            return Ok();
        }
    }
}