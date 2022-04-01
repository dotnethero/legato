using Microsoft.AspNetCore.Mvc;

namespace Legato.App.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("status")]
        public string GetStatus() => "OK";

        [HttpPost("ping")]
        public string Ping() => "OK";
    }
}
