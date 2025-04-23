namespace lexora_api.Models.Dto.Auth;

public class RegisterDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }

    public required string Username { get; set; }
}
public class LoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}
