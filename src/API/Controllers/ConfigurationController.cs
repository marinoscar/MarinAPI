using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Controller for configuration-related operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        /// <summary>
        /// Retrieves a configuration collection by name.
        /// </summary>
        /// <param name="name">The name of the configuration collection.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the status, collection name, data, and duration.
        /// </returns>
        [ApiKeyAuthorize]
        [HttpGet("getcollection")]
        public Task<IActionResult> GetCollection(string name)
        {
            return Task.FromResult<IActionResult>(Ok(new
            {
                status = "Success",
                collection = name,
                data = new { key = "VeryLongKey" },
                duration = "0.123 seconds"
            }));
        }
    }
}
