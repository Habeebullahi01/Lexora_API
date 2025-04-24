using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lexora_api.Controllers;

[ApiController]
[Authorize(Roles = "Librarian")]
[Route("api/book")]
public class BookController : ControllerBase
{

    [HttpGet]
    public IActionResult D()
    {
        return Ok();
    }

}