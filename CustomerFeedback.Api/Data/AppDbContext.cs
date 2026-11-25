using CustomerFeedback.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CustomerFeedback.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.CustomerEmail)
                .IsRequired()
                .HasMaxLength(320);

            entity.Property(e => e.Rating)
                .IsRequired();

            entity.Property(e => e.Comment)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(e => e.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(e => e.CreatedAtUtc);
        });
    }
}