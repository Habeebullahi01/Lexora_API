using lexora_api.Models;

namespace lexora_api.Services;

public interface IBookService
{
    public Task<Book?> AddBook(Book book);
    // public Book? EditBook(int id, Book book);
    // public List<Book> GetBooks(Filter? filter);
}

public class Filter
{
    public string? SortBy { get; set; }
}

public class BookService : IBookService
{

    public async Task<Book?> AddBook(Book book)
    {
        // derive the non required properties of the book from the created book
        book.AvailableQuantity = book.TotalQuantity;
        return book;
    }
}