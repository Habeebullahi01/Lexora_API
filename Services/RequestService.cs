using lexora_api.Data;
using lexora_api.Models;
using lexora_api.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace lexora_api.Services;

public interface IRequestService
{
    /// <summary>
    /// Retrieves a list of BorrowRequests
    /// </summary>
    /// <returns>List of BorrowRequest</returns>
    public Task<List<BorrowRequest>> GetBorrowRequestsAsync();
    /// <summary>
    /// Retrieves a list of BorrowRequest with pagination data.
    /// </summary>
    /// <param name="page"></param>
    /// <param name="limit"></param>
    /// <param name="order"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public Task<RequestsResponse> GetBorrowRequestsAsync(int page, int limit, Order order, RequestStatus status);

    public Task<CreationResponse> CreateRequest(BorrowRequest request);
    public Task<bool> CheckPendingRequest(string userId);
    public Task<ApprovalResponse> Approve(int requestId, string librarianId);
    /// <summary>
    /// Retrieve a single BorrowRequest object by it's ID
    /// </summary>
    /// <param name="requestId">The Id of the BorrowRequest</param>
    /// <returns>A BorrowRequest, or null if none is found with the Id</returns>
    public Task<BorrowRequest?> GetBorrowRequest(int requestId);

    /// <summary>
    /// Retrieve all BorrowRequest made by a user. User Id is gotten from the User object (populated from the JWT in th Authorization header)
    /// </summary>
    /// <returns>A Paginated list of BorrowRequests. The pagination is only to adhere to standards, no provisions are made for retriving pages</returns>
    public Task<RequestsResponse> RetrieveUserRequests(string userId);

    /// <summary>
    /// Reject a request
    /// </summary>
    /// <param name="requestId">Id of the request to be rejected</param>
    /// <param name="librarianId">Id of the librarian who initiated this action</param>
    /// <returns>A rejection response object</returns>
    public Task<RejectionResponse> RejectRequest(int requestId, string librarianId);

    /// <summary>
    /// Marks a BorrowRequest as returned
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<BorrowRequest?> Return(int id);

}

public class RequestService(AppDbContext context, IBookService bookService) : IRequestService
{
    private readonly AppDbContext _context = context;
    private readonly IBookService _bookService = bookService;
    public async Task<List<BorrowRequest>> GetBorrowRequestsAsync()
    {
        var retrieved = await _context.Requests.OrderBy(r => r.Id).Include(r => r.Books).ToListAsync();
        return retrieved;
    }
    public async Task<RequestsResponse> GetBorrowRequestsAsync(int page, int limit, Order order, RequestStatus status)
    {
        // var response = new RequestsResponse() {}
        var reqs = _context.Requests;
        limit = limit <= 0 ? 1 : limit;
        IQueryable<BorrowRequest> orderedRequests;
        if (order.Equals(Order.Ascending))
        {
            orderedRequests = reqs.OrderBy(r => r.Id).Where(r => r.Status == status).Include(r => r.Books);
        }
        else
        {
            orderedRequests = reqs.OrderByDescending(r => r.Id).Where(r => r.Status == status).Include(r => r.Books);
        }
        int totalItems = orderedRequests.Count();

        int totalPages;
        totalPages = (int)Math.Ceiling((decimal)totalItems / limit);

        var taken = orderedRequests.Skip((page - 1) * limit).Take(limit);

        var retrieved = await taken.ToListAsync();

        RequestsResponse response = new() { Requests = retrieved, ItemsPerPage = limit, CurrentPage = page, TotalItems = totalItems, TotalPages = totalPages };
        // var retrieved = await _context.Requests.OrderBy(r => r.Id).Include(r => r.Books).ToListAsync();
        return response;
    }

    public async Task<CreationResponse> CreateRequest(BorrowRequest request)
    {
        // create the CreationResponse object here and return it to the controller
        CreationResponse response = new();
        // validate user's pending request status
        if (await CheckPendingRequest(request.ReaderId))
        {
            response.HasPendingRequest = true;
            response.Reason = "User has a pending request";
            return response; // return
        }

        try
        {
            await _context.Requests.AddAsync(request);
            response.Request = request;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return response;
    }

    public async Task<bool> CheckPendingRequest(string userId)
    {
        var pendingRequest = await _context.Requests.FirstOrDefaultAsync(r => r.ReaderId == userId && r.Status == RequestStatus.Pending);
        if (pendingRequest != null)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    public async Task<ApprovalResponse> Approve(int requestId, string librarianId)
    {
        // retrieve request
        BorrowRequest? request = await _context.Requests.Include(r => r.Books).FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            ApprovalResponse response = new()
            {
                RequestNotFound = true,
                Reason = "No request with the provided ID was found."
            };
            return response;
        }
        // ensure request is pending or rejected
        if (request.Status != RequestStatus.Pending && request.Status != RequestStatus.Rejected)
        {
            ApprovalResponse response = new()
            {
                RequestNotPending = true,
                Reason = "Request is either approved or returned."
            };
            return response;
        }
        // ensure reader has no outstanding penalty
        var outstanding = _context.Requests.FirstOrDefault(r => r.ReaderId == request.ReaderId && r.PenaltyIncurred > 0);
        if (outstanding != null)
        {
            ApprovalResponse response = new()
            {
                OutstandingPenalty = true,
                Reason = "User has outstanding penalty."
            };
            return response;
        }
        // ensure books are available (book.AvailableQuantity > 0)
        List<Book> requestedBooks = request.Books;
        List<Book> unavailableBooks = [];
        List<int> bookIds = [];
        int uB = 0;
        foreach (Book book in requestedBooks)
        {
            bookIds.Add(book.Id);
            if (book.AvailableQuantity < 1)
            {
                uB += 1;
                unavailableBooks.Add(book);
            }
        }

        // do not approve if any book is unavailable
        if (uB == 0)
        {
            request.Status = RequestStatus.Approved;
            request.LibrarianID = librarianId;
            var startDate = DateTime.UtcNow;
            var endDate = DateTime.UtcNow.AddDays(request.Duration);
            request.StartDate = DateOnly.FromDateTime(startDate);
            request.EndDate = DateOnly.FromDateTime(endDate);
            await _context.SaveChangesAsync();
            ApprovalResponse response = new()
            {
                Request = request,
                // Reason = "At least one book in the request is unavailable"
            };
            // reduce the quantity of available books
            Console.WriteLine(bookIds.Count);
            _bookService.BorrowBooks(bookIds);
            return response;
        }
        else
        {
            ApprovalResponse response = new()
            {
                UnavailableBooks = uB,
                Reason = $"The request contains {unavailableBooks.Count} unavailable book(s)"

            };
            return response;
        }
        // 
    }

    public async Task<RejectionResponse> RejectRequest(int requestId, string librarianId)
    {
        var request = await _context.Requests.FirstOrDefaultAsync(r => r.Id == requestId);
        RejectionResponse res = new() { Request = request };
        if (request == null)
        {
            res.RequestNotFound = true;
            res.Reason = "The request was not found";
            return res;
        }
        request.Status = RequestStatus.Rejected;
        request.LibrarianID = librarianId;
        await _context.SaveChangesAsync();
        res.Request = request;
        return res;
    }

    public async Task<BorrowRequest?> GetBorrowRequest(int requestId)
    {
        var r = await _context.Requests.Include(r => r.Books).SingleOrDefaultAsync(r => r.Id == requestId);
        return r;
    }

    public async Task<RequestsResponse> RetrieveUserRequests(string userId)
    {
        var r = await _context.Requests.Where(r => r.ReaderId == userId).OrderBy(r => r.Id).Include(r => r.Books).ToListAsync();
        RequestsResponse res = new() { Requests = r, CurrentPage = 1, ItemsPerPage = r.Count, TotalItems = r.Count, TotalPages = 1 };
        return res;
    }

    public async Task<BorrowRequest?> Return(int id)
    {
        // retrieve request
        var req = await _context.Requests.Include(r => r.Books).FirstOrDefaultAsync(r => r.Id == id);
        if (req == null || req.Status != RequestStatus.Approved)
        {
            return null;
        }
        List<int> bookIds = [];
        // reset available books
        foreach (Book book in req.Books)
        {
            bookIds.Add(book.Id);
            Console.WriteLine($"Added book with Id: {book.Id}");
        }
        Console.WriteLine(bookIds.ToArray().ToString());
        _bookService.ReturnBooks(bookIds);
        // set return date
        req.ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow);
        // calculate and set penalty
        // penalty  is $1 per book per day
        var bookCount = req.Books.Count;
        var defaultedDays = DateTime.Now.CompareTo(req.EndDate.ToDateTime(TimeOnly.MinValue));
        var endDate = req.EndDate.ToDateTime(TimeOnly.MinValue);
        var daysLate = (DateTime.Now.Date - endDate.Date).Days;
        if (daysLate <= 0)
        {
            daysLate = 0;
        }
        decimal penalty = bookCount * daysLate;
        Console.WriteLine($"Penalty: {penalty}");
        req.PenaltyIncurred = penalty;
        // mark as returned
        req.Status = RequestStatus.Returned;
        await _context.SaveChangesAsync();
        return req;
    }
}

public abstract class CustomResponse
{
    public BorrowRequest? Request { get; set; }
    public int UnavailableBooks { get; set; }
    // public bool RequestNotFound;
    // public bool RequestNotPending;
    public string? Reason { get; set; }
    public abstract bool Succeeded();
}
public class ApprovalResponse : CustomResponse
{
    public bool OutstandingPenalty { get; set; }
    public bool RequestNotPending { get; set; }
    public bool RequestNotFound { get; set; }
    public override bool Succeeded() { return UnavailableBooks == 0 && !RequestNotFound && !RequestNotPending && !OutstandingPenalty; }

}
public class RejectionResponse : CustomResponse
{
    // public bool RequestNotPending { get; set; }
    public bool RequestNotFound { get; set; }
    public override bool Succeeded() { return !RequestNotFound; }

}

public class CreationResponse : CustomResponse
{
    public bool HasPendingRequest { get; set; }
    public bool NoBooks { get; set; }
    public override bool Succeeded()
    {
        return UnavailableBooks == 0 && !NoBooks && !HasPendingRequest;
    }
}