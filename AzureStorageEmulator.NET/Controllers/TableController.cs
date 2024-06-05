using Microsoft.AspNetCore.Mvc;

namespace AzureStorageEmulator.NET.Controllers
{
    [Route("devstoreaccount1")]
    [ApiController]
    [Host("*:10002")]
    public class TableController : ControllerBase
    {
    }
}