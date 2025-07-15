using API.Config;
using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Net.NetworkInformation;

namespace API.Controllers
{
    /// <summary>
    /// Provides health check endpoints for the API.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Returns a simple liveness check indicating the API is running.
        /// </summary>
        /// <returns>
        /// 200 OK with status and current UTC timestamp.
        /// </returns>
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new
            {
                status = "Alive",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Performs a readiness check, including a database connectivity test.
        /// </summary>
        /// <returns>
        /// 200 OK if the API and database are healthy, 503 if the database is unreachable.
        /// </returns>
        [HttpGet("ready")]
        public IActionResult Ready()
        {
            var dbStatus = "Unknown";
            try
            {
                var connString = DbHelper.GetConnectionString();

                using var conn = new NpgsqlConnection(connString);
                conn.Open();
                dbStatus = "Connected";
            }
            catch (Exception ex)
            {
                dbStatus = $"Unreachable: {ex.Message}";
                return StatusCode(503, new
                {
                    status = "Unhealthy",
                    db = dbStatus,
                    timestamp = DateTime.UtcNow
                });
            }

            return Ok(new
            {
                status = "Healthy",
                db = dbStatus,
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Checks if the provided API key is valid.
        /// Requires a valid API key in the request.
        /// </summary>
        /// <returns>
        /// 200 OK if the API key is valid.
        /// </returns>
        [ApiKeyAuthorize]
        [Tags("Secured")]
        [HttpGet("apikeycheck")]
        public IActionResult ApiKeyCheck()
        {
            return Ok(new
            {
                status = "Valid Key",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
