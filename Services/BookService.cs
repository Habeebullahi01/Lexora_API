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
    // public List<Book> GetBooks(Filter? filter);
}

public class Filter
{
    public string? SortBy { get; set; }
}

public class BookService(AppDbContext context) : IBookService
{
    private readonly AppDbContext _context = context;

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
        if (!string.IsNullOrWhiteSpace(book.ISBN?.Trim()))
        {
            bookToModify.ISBN = book.ISBN;
        }
        if (!string.IsNullOrWhiteSpace(book.Title?.Trim()))
        {
            bookToModify.Title = book.Title;
        }
        // bookToModify.Author = book.Author;
        // bookToModify.Description = book.Description;
        // bookToModify.TotalQuantity = book.TotalQuantity;
        book.ISBN = book.ISBN;
        await _context.SaveChangesAsync();
        return bookToModify;
    }
}