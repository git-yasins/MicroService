using Microsoft.AspNetCore.Mvc;

namespace Contact.API.Controllers {
    /// <summary>
    /// Consul服务发现健康检查
    /// </summary>
    [Route ("[Controller]")]
    public class HealthCheckController : ControllerBase {
        [HttpGet ("")]
        [HttpHead ("")]
        public IActionResult Ping () {
            return Ok ();
        }
    }
}