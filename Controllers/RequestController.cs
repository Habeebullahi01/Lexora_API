using System.ComponentModel;
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
    [ProducesResponseType<RequestsResponse>(200)]
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
        CustomResponse res = await _requestService.Approve(id, librarianId);
        if (!res.Succeeded)
        {
            return BadRequest(new ProblemDetails() { Detail = res.Reason });
        }
        else
        {
            return Ok(res.Request);
        }
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

    [HttpPost]
    [Authorize(Roles = "Reader")]
    [EndpointDescription("For creating a new borrow request. Only readers can use this endpoint.")]
    [ProducesResponseType<BorrowRequest>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> Create([FromBody][Description("A list/array of IDs of the books to be borrowed, and the number of days for which they are requested.")] NewRequestDto newRequestDto)
    {
        // extract the user id from ?
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        // fetch the requested books from book service
        List<Book> requestedBooks = await _bookService.GetBooks(newRequestDto.Books);
        if (requestedBooks.Count <= 0)
        {
            Console.WriteLine("requested books is empty");
        }
        else
        {
            Console.WriteLine("From Create action in reQuest controller");
            Console.WriteLine(requestedBooks[0].Author);
        }

        // nbr.Books.AddRange(requestedBooks);
        BorrowRequest nbr = new() { Duration = newRequestDto.Duration, ReaderId = userId!.Value };
        nbr.Books.AddRange(requestedBooks);
        nbr.StartDate = new DateOnly();
        nbr.EndDate = new DateOnly().AddDays(nbr.Duration);

        // Check for existing pending request by user
        bool userHasPendingRequest = await _requestService.CheckPendingRequest(userId.Value);
        if (!userHasPendingRequest)
        {
            // Add Request to Requests Table
            var a = await _requestService.CreateRequest(nbr);
            if (a != null)
            {

                return Ok(a);
            }
            else
            {
                return BadRequest(nbr);
            }
        }
        else
        {
            return BadRequest(new ProblemDetails() { Detail = "User already has a pending request." });
        }
    }


}