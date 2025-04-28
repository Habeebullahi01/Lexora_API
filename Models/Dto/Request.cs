namespace lexora_api.Models.Dto;

public class NewRequestDto
{
    public List<int> Books { get; set; } = [];
    public int Duration { get; set; }
}