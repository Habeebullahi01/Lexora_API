namespace lexora_api.Models;

public class Book
{
    public int Id { get; set; }

    public required string Title { get; set; }

    public required string ISBN { get; set; }
    public required string Author { get; set; }
    public DateOnly PublicationDate { get; set; }

    public required string Description { get; set; }
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
}