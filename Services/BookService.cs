using System.Threading.Tasks;
using lexora_api.Data;
using lexora_api.Models;
using lexora_api.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace lexora_api.Services;

public interface IBookService
{
    public Task<Book?> AddBook(Book book);

    /// <summary>
    /// This is used to edit a Book
    /// </summary>
    /// <param name="id">The Id of the existing Book. Possibly from the route url.</param>
    /// <param name="book">An object containing possibly null values of modifiable properties of Book</param>
    /// <returns></returns>
    public Task<Book?> EditBook(int id, UpdateBookDto book);

    /// <summary>
    /// Retrieve a list of books
    /// </summary>
    /// <returns>List of Books</returns>
    public Task<List<Book>> GetBooks();

    /// <summary>
    /// Retrieve a list of Books
    /// </summary>
    /// <param name="filter">Criteria to filter or sort</param>
    /// <returns>List of Books</returns>
    public Task<BooksResponse> GetBooks(Filter filter, int pageNumber, int limit);

    public Task<List<Book>> GetBooks(List<int> bookIds);

    /// <summary>
    /// Reduces the AvailableQuantity property of a series of Books by 1 each.
    /// </summary>
    /// <param name="bookIds">A List of Ids corresponding to those of the Books to be operated on</param>
    public void BorrowBooks(List<int> bookIds);

}

public class Filter
{
    public SortCriteria? SortBy { get; set; }
    public Order? Order { get; set; } = Services.Order.Ascending;
    // public int PerPage { get; set; }
    // public int PageNumber { get; set; }
}

public enum SortCriteria
{
    PublicationDate = 1,
    Title,
    Author
}

public enum Order
{
    Descending,
    Ascending,
}

public class BookService(AppDbContext context) : IBookService
{
    private readonly AppDbContext _context = context;
    public async Task<List<Book>> GetBooks()
    {
        var books = await _context.Books.OrderBy(b => b.Id).ToListAsync();
        return books;
    }

    public async Task<BooksResponse> GetBooks(Filter filter, int pageNumber, int limit)
    {
        var books = _context.Books;
        // minimun limit is 1
        limit = limit <= 0 ? 1 : limit;

        IQueryable<Book> orderedBooks;
        if (filter.Order > 0)
        {
            // Ascending
            orderedBooks = filter.SortBy switch
            {
                SortCriteria.PublicationDate => books.OrderBy(b => b.PublicationDate),
                SortCriteria.Title => books.OrderBy(b => b.Title),
                SortCriteria.Author => books.OrderBy(b => b.Author),
                _ => books.OrderBy(b => b.Id)
            };
        }
        else
        {
            // Descending
            orderedBooks = filter.SortBy switch
            {
                SortCriteria.PublicationDate => books.OrderByDescending(b => b.PublicationDate),
                SortCriteria.Title => books.OrderByDescending(b => b.Title),
                SortCriteria.Author => books.OrderByDescending(b => b.Author),
                _ => books.OrderByDescending(b => b.Id)
            };
        }

        var totalItems = orderedBooks.Count();
        int totalPages;

        if (totalItems > limit)
        {
            totalPages = totalItems / limit % 2 switch
            {
                > 0 => totalItems / limit + 1,
                _ => totalItems / limit
            };
        }
        else
        {
            totalPages = 1;
        }

        var skipped = orderedBooks.Skip(pageNumber - 1 * limit);
        var picked = await skipped.Take(limit).ToListAsync();
        return new BooksResponse() { Books = picked, CurrentPage = pageNumber, ItemsPerPage = limit, TotalItems = totalItems, TotalPages = totalPages };

        // return await picked.ToListAsync();
    }

    public async Task<List<Book>> GetBooks(List<int> bookIds)
    {
        List<Book> books = [];

        foreach (int id in bookIds)
        {
            var retrivedBook = await _context.Books.FindAsync(id);
            if (retrivedBook != null)
            {
                // Console.WriteLine(retrivedBook.Author);
                books.Add(retrivedBook);
            }
        }

        return books;
    }


    public async Task<Book?> AddBook(Book book)
    {
        // derive the non required properties of the book from the created book
        book.AvailableQuantity = book.TotalQuantity;
        try
        {

            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
            return book;
        }
        catch
        {
            // throw;
            return null;
        }

    }

    public async Task<Book?> EditBook(int id, UpdateBookDto book)
    {
        var bookToModify = await _context.Books.SingleOrDefaultAsync<Book>(b => b.Id == id);
        if (bookToModify == null)
        {
            return null;
        }
        if (!string.IsNullOrWhiteSpace(book.Author?.Trim()))
        {
            bookToModify.Author = book.Author;
        }
        if (!string.IsNullOrWhiteSpace(book.Description?.Trim()))
        {
            bookToModify.Description = book.Description;
        }
        if (!string.IsNullOrWhiteSpace(book.Title?.Trim()))
        {
            bookToModify.Title = book.Title;
        }

        if (!string.IsNullOrWhiteSpace(book.PublicationDate.ToString()) && book.PublicationDate.Year > 2025)
        {
            bookToModify.PublicationDate = book.PublicationDate;
        }

        await _context.SaveChangesAsync();
        return bookToModify;
    }


    public async void BorrowBooks(List<int> bookIds)
    {
        Console.WriteLine("Borrowing books...");
        List<Book> books = await _context.Books.Where(b => bookIds.Contains(b.Id)).ToListAsync();
        Console.WriteLine($"There are {bookIds.Count} books to work on");
        Console.WriteLine($"There are {books.Count} books to work on");
        foreach (Book book in books)
        {
            Console.WriteLine(book.Author);
            Console.WriteLine(book.AvailableQuantity);
            book.AvailableQuantity -= 1;
            Console.WriteLine(book.AvailableQuantity);
        }
        await _context.SaveChangesAsync();
    }
}