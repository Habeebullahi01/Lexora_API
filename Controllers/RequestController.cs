using System.ComponentModel;
using System.Security.Claims;
using System.Text.Json;
using lexora_api.Models;
using lexora_api.Models.Dto;
using lexora_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lexora_api.Controllers;

[ApiController]
[Authorize]
[Route("api/requests")]
public class RequestController(IRequestService requestService, IBookService bookService) : ControllerBase
{
    private readonly IRequestService _requestService = requestService;
    private readonly IBookService _bookService = bookService;

    [HttpGet]
    [Authorize(Roles = "Librarian")]
    [ProducesResponseType<RequestsResponse>(200, "application/json")]
    [EndpointSummary("Retrieves a paginated list of Requests")]
    [EndpointDescription(@"The 'status' field of the query parameters is representative of the status of the request which is desired. 0 = Pending, 1 = Approved, 2 = Rejected, 3 = Returned. The default is 0 which corresponds to Pending requests.
    The order field specifies the order; 0 = Descending (newest first) and 1 = Ascending (oldest first)")]
    public async Task<IActionResult> RetrieveRequests(int size = 10, int page = 1, Order order = Order.Descending, RequestStatus status = RequestStatus.Pending)
    {
        // var paginatedResponse = await _requestService.
        return Ok(await _requestService.GetBorrowRequestsAsync(page, size, order, status));
    }

    [HttpPost("approve/{id}")]
    [Authorize(Roles = "Librarian")]
    [EndpointDescription("For Approving a request. Only Librarians can use this endpoint.")]
    [ProducesResponseType<BorrowRequest>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ApproveRequest(int id)
    {
        var librarianId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;
        ApprovalResponse res = await _requestService.Approve(id, librarianId);
        if (!res.Succeeded())
        {
            return BadRequest(new ProblemDetails() { Detail = res.Reason });
        }
        else
        {
            return Ok(res.Request);
        }
    }

    [HttpPost("action/{id}")]
    [Authorize(Roles = "Librarian")]
    [EndpointDescription("For acting on a request. Only Librarians can use this endpoint.")]
    [ProducesResponseType<RejectionResponse>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<RejectionResponse>(StatusCodes.Status400BadRequest, "application/json")]
    [ProducesResponseType<BadRequestResult>(400, "application/json")]
    public async Task<IActionResult> ActOnRequest(int id, [FromQuery] string action)
    {
        switch (action)
        {
            case "approve":
            case "reject":
                break;
            default:
                return BadRequest("Invalid action value");
        }
        var librarianId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        if (action == "approve")
        {

            ApprovalResponse res = await _requestService.Approve(id, librarianId);
            if (!res.Succeeded())
            {
                return BadRequest(new ProblemDetails() { Detail = res.Reason });
            }
            else
            {
                return Ok(res.Request);
            }
        }
        if (action == "reject")
        {
            RejectionResponse res = await _requestService.RejectRequest(id, librarianId);
            if (res.Succeeded())
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }
        }
        return BadRequest("Invalid action value");
    }


    [HttpGet("{id}")]
    [EndpointSummary("Retrieves a single Borrow Request")]
    [ProducesResponseType<BorrowRequest>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> GetOne(int id)
    {
        BorrowRequest? b = await _requestService.GetBorrowRequest(id);
        if (b == null)
        {
            return NotFound();
        }
        else
        {
            return Ok(b);
        }
    }

    [HttpGet("user")]
    [ProducesResponseType<RequestsResponse>(200, "application/json")]
    [EndpointSummary("Retrieves a list of requests made by an authenticated user")]
    [Authorize(Roles = "Reader")]
    public async Task<IActionResult> GetUserRequest()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }
        var res = await _requestService.RetrieveUserRequests(userId);
        return Ok(res);
    }

    [HttpPost]
    [Authorize(Roles = "Reader")]
    [EndpointDescription("For creating a new borrow request. Only readers can use this endpoint.")]
    [ProducesResponseType<BorrowRequest>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> Create([FromBody][Description("A list/array of IDs of the books to be borrowed, and the number of days for which they are requested.")] NewRequestDto newRequestDto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        CreationResponse a = new();
        BorrowRequest nbr = new() { Duration = newRequestDto.Duration, ReaderId = userId!.Value };
        // nbr.StartDate = new DateOnly();
        // nbr.EndDate = new DateOnly().AddDays(nbr.Duration);
        // extract the user id from ?
        // fetch the requested books from book service
        List<Book> requestedBooks = await _bookService.GetBooks(newRequestDto.Books);
        if (newRequestDto.Books.Count == 0)
        {
            // a.Reason = "No books were requested";
            // a.NoBooks = true;
            return BadRequest(new ProblemDetails() { Detail = "No books were requested." });
        }
        if (requestedBooks.Count <= 0)
        {
            // a.UnavailableBooks = newRequestDto.Books.Count - requestedBooks.Count;
            // a.Reason = "No requested book is available.";
            Console.WriteLine("requested books is empty");
            return BadRequest(new ProblemDetails() { Detail = "No requested book is available" });
        }
        nbr.Books.AddRange(requestedBooks);

        // send new instance of bookrequest to service.
        var aa = await _requestService.CreateRequest(nbr);
        Console.WriteLine(aa.Succeeded());
        Console.WriteLine(aa.Reason);
        Console.WriteLine(aa.HasPendingRequest);

        if (aa.Succeeded())
        {
            return Ok(aa);
        }
        else
        {
            return BadRequest(aa);
        }
    }


}