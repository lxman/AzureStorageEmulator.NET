using AzureStorageEmulator.NET.Common;
using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Host("*:10010")]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {

            return Ok(new Status());
        }
    }
}
