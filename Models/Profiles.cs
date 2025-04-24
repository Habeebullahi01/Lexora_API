namespace lexora_api.Models;

public class ReaderProfile
{
    public int Id { get; set; }
    public decimal Penalty { get; set; }

    public required string UserId { get; set; }
    public required ApplicationUser User { get; set; }
}

public class LibrarianProfile
{
    public int Id { get; set; }

    public required string UserId { get; set; }
    public required ApplicationUser User { get; set; }
}