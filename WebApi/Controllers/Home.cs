using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    public class HomeController : ControllerBase
    {
        // Endpoint GET /ping
        [HttpGet("/ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "Server is Running" });
        }
    }
}