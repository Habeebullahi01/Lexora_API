using Microsoft.VisualBasic;

namespace lexora_api.Models;

public class BorrowRequest
{
    public int Id { get; set; }
    public RequestStatus Status { get; set; }
    public decimal PenaltyIncurred { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public DateOnly PickUpDate { get; set; }
    public DateOnly ReturnDate { get; set; }
    public int Duration { get; set; }
    public int ReaderId { get; set; }
    // public required ReaderProfile Reader { get; set; }
    public int? LibrarianID { get; set; }
    // public LibrarianProfile? Librarian { get; set; }
    public ICollection<Book> Books { get; } = [];
}

public enum RequestStatus
{
    Pending,
    Approved,
    Rejected,
    Returned
}