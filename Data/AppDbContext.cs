using lexora_api.Models;
using Microsoft.EntityFrameworkCore;

namespace lexora_api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
}