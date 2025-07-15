using API.Config;
using API.Filters;
using BCCR.TipoDeCambio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for handling exchange rate operations with BCCR.
    /// </summary>
    [Route("api/[controller]")]
    [ApiKeyAuthorize]
    [ApiController]
    [Tags("Secured")]
    public class TipoDeCambioController : ControllerBase
    {
        /// <summary>
        /// Represents credentials required for BCCR API access.
        /// </summary>
        /// <param name="email">The email associated with the BCCR account.</param>
        /// <param name="token">The API token for BCCR access.</param>
        private record Credentials(string email, string token);

        /// <summary>
        /// Retrieves exchange rates from the database for the specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range (inclusive).</param>
        /// <param name="endDate">The end date of the range (inclusive).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the status, retrieved values, operation duration, and the date range.
        /// </returns>
        /// <response code="200">Returns the exchange rates and operation details.</response>
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
                duration = stopWatch.Elapsed.ToHumanReadableString(),
                startDate,
                endDate
            });
        }

        /// <summary>
        /// Fetches exchange rates from BCCR and persists them to the database for the specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range (inclusive).</param>
        /// <param name="endDate">The end date of the range (inclusive).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the status, number of affected rows, operation duration, and the date range.
        /// </returns>
        /// <response code="200">Returns the result of the persistence operation.</response>
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
                duration = stopWatch.Elapsed.ToHumanReadableString(),
                startDate,
                endDate
            });
        }

        /// <summary>
        /// Retrieves BCCR API credentials from environment variables or uses default values.
        /// </summary>
        /// <returns>
        /// A <see cref="Credentials"/> record containing the email and token.
        /// </returns>
        private Credentials GetCredentials()
        {
            var email = Environment.GetEnvironmentVariable("BCCR_EMAIL") ?? "oscar@marin.cr";
            var token = Environment.GetEnvironmentVariable("BCCR_TOKEN") ?? "NCIR1R2RRM";
            return new Credentials(email, token);
        }
    }
}
