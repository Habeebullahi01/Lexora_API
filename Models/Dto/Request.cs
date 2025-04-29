namespace lexora_api.Models.Dto;

public class NewRequestDto
{
    public List<int> Books { get; set; } = [];
    public int Duration { get; set; }
}

public class RequestsResponse : BulkResponse
{
    public required List<BorrowRequest> Requests { get; set; }
}