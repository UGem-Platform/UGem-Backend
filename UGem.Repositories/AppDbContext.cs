using Microsoft.EntityFrameworkCore;
using UGem.Repositories.Entity;

namespace UGem.Repositories;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<AffiliateLink> AffiliateLinks { get; set; }
    public DbSet<Admin> Admin { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewDetail> ReviewDetails { get; set; }
    public DbSet<Reviewer> Reviewers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Category> Categories { get; set; }
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(255);

            builder.HasIndex(u => u.Email)
                   .IsUnique();

            builder.Property(u => u.FullName)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(u => u.PasswordHash)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(u => u.PhoneNumber)
                   .HasMaxLength(20);

            builder.Property(u => u.AvatarUrl)
                   .HasMaxLength(500);

            builder.HasOne(u => u.Admin)
                   .WithOne(a => a.User)
                   .HasForeignKey<Admin>(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@ugem.com",
                FullName = "System Administrator",
                PasswordHash = "swccwevwvwvw",
                PhoneNumber = "0123456789",
                IsActive = true
            }
        );
        
        modelBuilder.Entity<Admin>(builder =>
        {
            builder.Property(a => a.Permissions)
                   .HasMaxLength(1000);
            
        });
        
        modelBuilder.Entity<Staff>(builder =>
        {
            builder.Property(s => s.HiredAt)
                   .IsRequired();
        });
        
        modelBuilder.Entity<AffiliateLink>(builder =>
        {
            builder.Property(a => a.LinkCode)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(a => a.LinkCode)
                   .IsUnique();
        });
        
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

            builder.HasOne(r => r.Order)
                .WithOne(o => o.Review)
                .HasForeignKey<Review>(r => r.Id) // Assuming Review has a foreign key to Order
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
