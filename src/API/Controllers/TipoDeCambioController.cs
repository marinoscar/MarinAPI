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

        ///<summary>
        /// Retrieves exchange rates from BCCR for the specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range (inclusive).</param>
        /// <param name="endDate">The end date of the range (inclusive).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the status, retrieved rates, operation duration, and the date range.
        /// </returns>
        /// <response code="200">Returns the exchange rates and operation details.</response>
        [ApiKeyAuthorize]
        [HttpGet("getrates")]
        public async Task<IActionResult> GetRates(DateTime startDate, DateTime endDate)
        {
            var cred = GetCredentials();
            var exchangeRates = new ExchangeRateBCCR();
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var rates = await exchangeRates.GetExchangeRateAsync(cred.email, cred.token, startDate, endDate);
            return Ok(new
            {
                status = "Success",
                rates,
                duration = stopWatch.Elapsed.ToHumanReadableString(),
                startDate,
                endDate
            });
        }


        /// <summary>
        /// Retrieves exchange rates from the database for the specified date range.
        /// </summary>
        /// <param name="startDate">The start date of the range (inclusive).</param>
        /// <param name="endDate">The end date of the range (inclusive).</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the status, retrieved values, operation duration, and the date range.
        /// </returns>
        /// <response code="200">Returns the exchange rates and operation details.</response>
        [ApiKeyAuthorize]
        [HttpGet("getratesfromdb")]
        public async Task<IActionResult> GetRatesFromDb(DateTime startDate, DateTime endDate)
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
        [ApiKeyAuthorize]
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
        /// Generates an HTML chart representing exchange rates for the last 30 days ending at the specified date.
        /// </summary>
        /// <param name="date">The end date for the chart (inclusive). If null or invalid, defaults to today.</param>
        /// <returns>
        /// A <see cref="ContentResult"/> containing the generated HTML chart, with a status code of 200 and content type "text/html".
        /// </returns>
        /// <remarks>
        /// This endpoint retrieves exchange rate data for the 30-day period ending at the given date and returns a visual chart in HTML format.
        /// The operation duration is measured internally but not returned in the response.
        /// </remarks>
        /// <response code="200">Returns the HTML chart for the specified 30-day period.</response>
        [HttpGet("getratechart")]
        public async Task<ContentResult> GetRateChart(DateTime? date)
        {
            if (date == null || date.Value.Year <= 1970) date = DateTime.Today.Date;
            var stopWatch = System.Diagnostics.Stopwatch.StartNew();
            var startDate = date.Value.AddDays(-30).Date; // Default to last 30 days
            var endDate = date.Value.AddDays(1).Date; // Default to today
            var html = await HtmlParser.GetHtmlAsync(startDate, endDate);
            stopWatch.Stop();
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content = html
            };
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
