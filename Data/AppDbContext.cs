using lexora_api.Models;
using Microsoft.EntityFrameworkCore;

namespace lexora_api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<BorrowRequest> Requests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Required One-to-many: One ReaderProfile HasMany Requests WithOne Reader Fk Request.ReaderId
        // modelBuilder.Entity<ReaderProfile>().HasMany(r => r.Requests).WithOne(r => r.Reader).HasForeignKey(r => r.ReaderId);

        // Optional One-to-many: One LibrarianProfile HasMany Requests WithOne Librarian Fk Request.Librarian
        // modelBuilder.Entity<LibrarianProfile>().HasMany(l=>l.Requests).WithOne(r=>r.Librarian).HasForeignKey(r=>r.LibrarianID);

        // One-to-many: One request can have many Books: A join table called RequestBooks
        modelBuilder.Entity<BorrowRequest>()
        .HasMany(r => r.Books)
        .WithMany(b => b.Requests)
        .UsingEntity("RequestBooks");
    }
}