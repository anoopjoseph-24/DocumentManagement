using DMS.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DMS.Data;

public class DmsDbContext : IdentityDbContext<ApplicationUser>
{
    public DmsDbContext(DbContextOptions<DmsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>()
            .HasOne(category => category.Owner)
            .WithMany(user => user.Categories)
            .HasForeignKey(category => category.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>()
            .HasOne(document => document.Owner)
            .WithMany(user => user.Documents)
            .HasForeignKey(document => document.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Document>()
            .HasOne(document => document.Category)
            .WithMany(category => category.Documents)
            .HasForeignKey(document => document.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Category>()
            .HasIndex(category => new { category.OwnerId, category.Name })
            .IsUnique();
    }
}