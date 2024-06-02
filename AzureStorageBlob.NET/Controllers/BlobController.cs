using Microsoft.AspNetCore.Mvc;

namespace AzureStorageBlob.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Host("*:10000")]
    public class BlobController : ControllerBase
    {
    }
}
