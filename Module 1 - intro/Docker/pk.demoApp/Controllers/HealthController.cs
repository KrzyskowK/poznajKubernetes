using Microsoft.AspNetCore.Mvc;

namespace pk.demoApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("ok");
        }
    }
}
