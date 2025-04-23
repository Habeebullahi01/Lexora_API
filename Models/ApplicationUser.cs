using Microsoft.AspNetCore.Identity;

namespace lexora_api.Models;

public class ApplicationUser : IdentityUser
{
    public string Custom { get; set; } = "toucher";
}