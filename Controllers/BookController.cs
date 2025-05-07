using System.ComponentModel;
using lexora_api.Models;
using lexora_api.Models.Dto;
using lexora_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Net.Mime.MediaTypeNames;
namespace lexora_api.Controllers;

[Authorize(Roles = "Librarian")]
[ApiController]
[Route("api/book")]
public class BookController(IBookService bookService) : ControllerBase
{

    private readonly IBookService _bookService = bookService;

    [HttpGet]
    [AllowAnonymous]
    [EndpointSummary("Retrieve all Books")]
    [ProducesResponseType<BooksResponse>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> GetBooks([FromQuery] int page = 1, [FromQuery] SortCriteria sortBy = SortCriteria.PublicationDate, [FromQuery] int limit = 10)
    {
        var retrievedBooks = await _bookService.GetBooks(new Filter() { SortBy = sortBy }, page, limit);
        return Ok(retrievedBooks);
    }

    [HttpGet("few")]
    [AllowAnonymous]
    [EndpointSummary("Retrieve some Books")]
    [ProducesResponseType<List<Book>>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> GetSomeBooks([FromQuery] List<int> bookIds)
    {
        var retrievedBooks = await _bookService.GetBooks(bookIds);
        return Ok(retrievedBooks);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [EndpointSummary("Retrieve a single Book by its Id")]
    [ProducesResponseType<Book>(StatusCodes.Status200OK, "application/json")]
    public async Task<IActionResult> GetBook(int id)
    {
        var retrievedBook = await _bookService.GetBook(id);
        if (retrievedBook != null)
        {
            return Ok(retrievedBook);
        }
        else
        {
            return NotFound();
        }
    }

    [HttpPost]
    [EndpointSummary("Add a new book to the Library")]
    [EndpointDescription("This endpoint allows Librarians to add Books to the Library. ")]
    [ProducesResponseType<Book>(StatusCodes.Status201Created, "application/json")]
    // [ProducesResponseType(typeof(ModelStateDictionary.ValueEnumerator), StatusCodes.Status400BadRequest, "application/json")]
    public async Task<IActionResult> AddBook([FromBody][Description("Details needed for a new Book. All fields are required. The ISBN, once set, cannot be modified.")] CreateBookDto dto)
    {
        // Validate DTO
        ValidateBookDto(dto);

        if (!ModelState.IsValid)
        {
            // foreach (var k in ModelState.Keys)
            // {
            //     Console.WriteLine(k);
            //     // var e = ModelState.GetEnumerator();
            //     // if(ModelState)
            //     Console.WriteLine(string.Join(",", ModelState.GetValueOrDefault(k)?.Errors!));
            // }
            // for

            // Console.WriteLine(ModelState)
            // return BadRequest("Check the input and try again");
            // return BadRequest(new ProblemDetails() { Detail =  ModelSta});
            // return new BadRequestObjectResult(ModelState) { ContentTypes = { Application.Json } }; = !
            return BadRequest(ModelState);
        }

        try
        {
            // Create Book
            Book newBook = new() { Author = dto.Author, Title = dto.Title, ISBN = dto.ISBN, Description = dto.Description, TotalQuantity = dto.Quantity, DateAdded = DateTime.UtcNow, PublicationDate = dto.PublicationDate };
            Book? createdBook = await _bookService.AddBook(newBook);
            if (createdBook != null)
            {
                return Created($"https://thisAddress/{createdBook.Id}", createdBook);
            }
            else
            {
                return BadRequest();
            }
        }
        catch (Exception e)
        {
            return BadRequest($"An error occured: {e.Message}");
        }
    }


    [HttpPatch("{id}")]
    [EndpointSummary("Modify an existing Book")]
    [EndpointDescription("This is for modifying the details of a book. Currently the following properties of a Book are modifiable; Title, Auhor, PublicationDate, Description, and Quantity. The Book's ISBN is unchangeable because it is meant to be a unique identifier.")]
    [ProducesResponseType<Book>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ModifyBook(int id, [FromBody] UpdateBookDto dto)
    {
        var updatedBook = await _bookService.EditBook(id, dto);
        if (updatedBook == null)
        {
            return NotFound();
        }
        else
        {
            return Accepted("theurl", updatedBook);
        }
    }

    [HttpDelete("{id}")]
    [EndpointSummary("Delete an existing Book")]
    [ProducesResponseType<string>(StatusCodes.Status204NoContent, "application/json")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var bookRemovalSucceed = await _bookService.DeleteBook(id);
        if (bookRemovalSucceed)
        {
            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }
    private void ValidateBookDto(CreateBookDto dto)
    {
        if (dto.Quantity <= 0)
        {
            ModelState.AddModelError("Quantity", "You need to add at least 1 copy of the book");
        }
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            ModelState.AddModelError("Title", "Title should not be empty");
        }
        if (string.IsNullOrWhiteSpace(dto.ISBN))
        {
            ModelState.AddModelError("ISBN", "ISBN should not be empty");
        }
        if (string.IsNullOrWhiteSpace(dto.Author))
        {
            ModelState.AddModelError("Author", "Author should not be empty");
        }
        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            ModelState.AddModelError("Description", "Description should not be empty");
        }
        if (dto.PublicationDate.Year > 2025)
        {
            ModelState.AddModelError("PublicationDate", "Publication date can't be in the future");
        }
    }

}