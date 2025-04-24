using lexora_api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace lexora_api.Data;

public class AuthDbContext(DbContextOptions<AuthDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ReaderProfile> Readers { get; set; }
    public DbSet<LibrarianProfile> Librarians { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ReaderProfile>().HasOne(r => r.User).WithOne(u => u.ReaderProfile).HasForeignKey<ReaderProfile>(r => r.UserId);

        builder.Entity<LibrarianProfile>()
        .HasOne(l => l.User)
        .WithOne(u => u.LibrarianProfile)
        .HasForeignKey<LibrarianProfile>(l => l.UserId);
    }
}