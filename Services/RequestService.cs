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

    public Task<BorrowRequest> CreateRequest(BorrowRequest request);
    public Task<bool> CheckPendingRequest(string userId);
    public Task<CustomResponse> Approve(int requestId, string librarianId);


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

    public async Task<BorrowRequest> CreateRequest(BorrowRequest request)
    {
        try
        {
            await _context.Requests.AddAsync(request);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return request;
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

    public async Task<CustomResponse> Approve(int requestId, string librarianId)
    {
        // retrieve request
        BorrowRequest? request = await _context.Requests.Include(r => r.Books).FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
        {
            CustomResponse response = new()
            {
                RequestNotFound = true,
                Reason = "No request with the provided ID was found."
            };
            return response;
        }
        // ensure request is pending or rejected
        if (request.Status != RequestStatus.Pending && request.Status != RequestStatus.Rejected)
        {
            CustomResponse response = new()
            {
                RequestNotPending = true,
                Reason = "Request is either approved or returned."
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
            CustomResponse response = new()
            {
                Request = request,
                Reason = "At least one book in the request is unavailable"
            };
            // reduce the quantity of available books
            Console.WriteLine(bookIds.Count);
            _bookService.BorrowBooks(bookIds);
            return response;
        }
        else
        {
            CustomResponse response = new()
            {
                UnavailableBooks = uB,
                Reason = $"The request contains {unavailableBooks.Count} unavailable book(s)"

            };
            return response;
        }
        // 
    }

}

public class CustomResponse
{
    public BorrowRequest? Request;
    public int UnavailableBooks;
    public bool RequestNotFound;
    public bool RequestNotPending;
    public string? Reason;
    public bool Succeeded => UnavailableBooks == 0 && !RequestNotFound && !RequestNotPending;
}