using API.Filters;
using BCCR.TipoDeCambio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiKeyAuthorize]
    [ApiController]
    [Tags("Secured")]
    public class TipoDeCambioController : ControllerBase
    {
        private record Credentials(string email, string token);

        [HttpGet("getrates")]
        public async Task<IActionResult> GetRates(DateTime startDate, DateTime endDate)
        {
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var values = await DbHelper.GetExchangeRateAsync(startDate, endDate);
            stopWatch.Stop();
            return Ok(new
            {
                status = "Success",
                values,
                duration = stopWatch.ElapsedMilliseconds,
                startDate,
                endDate
            });
        }

        [HttpPost("persist")]
        public async Task<IActionResult> PersistAsync(DateTime startDate, DateTime endDate)
        {
            var cred = GetCredentials();
            var exchangeRates = new ExchangeRateBCCR();
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var results = await exchangeRates.GetExchangeRateAsync(cred.email, cred.token, startDate, endDate);
            var affectedRows = await DbHelper.UpsertExchangeRateAsync(results);
            return Ok(new
            {
                status = "Success",
                affectedRows,
                duration = stopWatch.ElapsedMilliseconds,
                startDate,
                endDate
            });
        }

        private Credentials GetCredentials()
        {
            var email = Environment.GetEnvironmentVariable("BCCR_EMAIL") ?? "oscar@marin.cr";
            var token = Environment.GetEnvironmentVariable("BCCR_TOKEN") ?? "NCIR1R2RRM";
            return new Credentials(email, token);
        }
    }
}
