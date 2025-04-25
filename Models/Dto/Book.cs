namespace lexora_api.Models.Dto;

public class CreateBookDto
{
    public required string Title { get; set; }
    public required string ISBN { get; set; }
    public required string Author { get; set; }
    public required DateOnly PublicationDate { get; set; }
    public required string Description { get; set; }
    public required int Quantity { get; set; }
}

public class UpdateBookDto
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public DateOnly PublicationDate { get; set; }
    public string? Description { get; set; }
    public int? Quantity { get; set; }
}