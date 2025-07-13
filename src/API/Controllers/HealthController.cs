using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok(new
            {
                status = "Alive",
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("ready")]
        public IActionResult Ready()
        {
            var dbStatus = "Unknown";
            try
            {
                var dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
                var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "postgres";
                var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "password";
                var dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? "5432";
                var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "logsdb";

                var connString = $"Host={dbServer};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";

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
    }
}
