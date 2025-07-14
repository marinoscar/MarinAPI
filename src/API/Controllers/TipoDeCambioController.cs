using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiKeyAuthorize]
    [ApiController]
    public class TipoDeCambioController : ControllerBase
    {
        [HttpGet("compraventa")]
        public IActionResult Live()
        {
            var email = Environment.GetEnvironmentVariable("BCCR_EMAIL");
            var token = Environment.GetEnvironmentVariable("BCCR_TOKEN");
            var values = BCCR.TipoDeCambio.IndicadoresEconomicosBCCR.ObtenerTipoDeCambio(email, token).GetAwaiter().GetResult();
            var str = values.ToString();
            return Ok(new
            {
                status = "Alive",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
