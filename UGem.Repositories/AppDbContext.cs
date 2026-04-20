using Microsoft.EntityFrameworkCore;
using UGem.Repositories.Entity;

namespace UGem.Repositories;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewDetail> ReviewDetails { get; set; }
    public DbSet<Reviewer> Reviewers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.Property(c => c.TotalCheckIns)
                .IsRequired();

            builder.HasOne(c => c.Reviewer)
                .WithOne(r => r.Customer)
                .HasForeignKey<Reviewer>(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Reviewer>(builder =>
        {
            builder.Property(r => r.Points)
                .IsRequired();

            builder.Property(r => r.Rank)
                .IsRequired();

            builder.Property(r => r.CommissionRate)
                .IsRequired();
        });
        
        modelBuilder.Entity<Review>(builder =>
        {
            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Content)
                .IsRequired();

            builder.HasOne(r => r.Reviewer)
                .WithMany(rev => rev.Reviews)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<ReviewDetail>(builder =>
        {
            builder.Property(rd => rd.DetailContent)
                .IsRequired();

            builder.Property(rd => rd.Rating)
                .IsRequired();

            builder.HasOne(rd => rd.Review)
                .WithMany(r => r.ReviewDetails)
                .HasForeignKey(rd => rd.Id) // Assuming ReviewDetail has a foreign key to Review
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}