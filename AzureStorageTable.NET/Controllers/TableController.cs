using Microsoft.AspNetCore.Mvc;

namespace AzureStorageTable.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Host("*:10002")]
    public class TableController : ControllerBase
    {
    }
}