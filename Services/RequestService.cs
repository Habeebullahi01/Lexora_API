using lexora_api.Data;
using lexora_api.Models;
using Microsoft.EntityFrameworkCore;

namespace lexora_api.Services;

public interface IRequestService
{
    public Task<List<BorrowRequest>> GetBorrowRequestsAsync();
    public Task<BorrowRequest> CreateRequest(BorrowRequest request);
    public Task<bool> CheckPendingRequest(string userId);

}

public class RequestService(AppDbContext context) : IRequestService
{
    private readonly AppDbContext _context = context;
    public async Task<List<BorrowRequest>> GetBorrowRequestsAsync()
    {
        var retrieved = await _context.Requests.OrderBy(r => r.Id).ToListAsync();
        return retrieved;
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
        var pendingRequest = await _context.Requests.FirstAsync(r => r.ReaderId == userId && r.Status == RequestStatus.Pending);
        if (pendingRequest != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}