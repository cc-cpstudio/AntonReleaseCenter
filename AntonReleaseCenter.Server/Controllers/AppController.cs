namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AppController : ControllerBase
{
    [HttpGet]
    public IActionResult Hello() => Ok("Hello from AntonReleaseCenter");
}