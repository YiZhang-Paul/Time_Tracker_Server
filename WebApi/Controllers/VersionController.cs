using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/v1/version")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public string GetVersion()
        {
            return "0.1.0";
        }
    }
}
