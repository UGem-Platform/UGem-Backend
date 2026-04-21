using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UGem.Repositories.Abtraction;
using UGem.Repositories.Entity;

namespace UGem.Repositories;

public class AppDbContext : DbContext
{
    private static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AdminProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Reviewer> Reviewers { get; set; }
    public DbSet<AffiliateLink> AffiliateLinks { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryDetail> CategoryDetails { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<WishlistDetail> WishlistDetails { get; set; }
    public DbSet<CheckIn> CheckIns { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewDetail> ReviewDetails { get; set; }
    public DbSet<Application> Applications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            ConfigureBaseEntity(builder);

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

            builder.Property(u => u.Role)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Customer");

            builder.HasOne(u => u.Admin)
                .WithOne(a => a.User)
                .HasForeignKey<Admin>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Staff)
                .WithOne(s => s.User)
                .HasForeignKey<Staff>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Customer)
                .WithOne(c => c.User)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Merchant)
                .WithOne(m => m.User)
                .HasForeignKey<Merchant>(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = AdminUserId,
                Email = "admin@ugem.com",
                FullName = "System Administrator",
                PasswordHash = "swccwevwvwvw",
                PhoneNumber = "0123456789",
                IsActive = true,
                Role = "Admin",
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Admin>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(a => a.Permissions)
                .HasMaxLength(1000);

            builder.HasIndex(a => a.UserId)
                .IsUnique();
        });

        modelBuilder.Entity<Admin>().HasData(
            new Admin
            {
                Id = AdminProfileId,
                UserId = AdminUserId,
                User = null!,
                Permissions = "*",
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Staff>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(s => s.HiredAt)
                .IsRequired();

            builder.HasIndex(s => s.UserId)
                .IsUnique();
        });

        modelBuilder.Entity<AffiliateLink>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(a => a.LinkCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(a => a.LinkCode)
                .IsUnique();

            builder.HasOne(a => a.Reviewer)
                .WithMany(r => r.AffiliateLinks)
                .HasForeignKey(a => a.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Merchant)
                .WithMany(m => m.AffiliateLinks)
                .HasForeignKey(a => a.MerchantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(c => c.TotalCheckIns)
                .IsRequired();

            builder.HasIndex(c => c.UserId)
                .IsUnique();

            builder.HasOne(c => c.Reviewer)
                .WithOne(r => r.Customer)
                .HasForeignKey<Reviewer>(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Wishlist)
                .WithOne(w => w.Customer)
                .HasForeignKey<Wishlist>(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reviewer>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(r => r.Points)
                .IsRequired();

            builder.Property(r => r.Rank)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.CommissionRate)
                .HasPrecision(5, 2);

            builder.HasIndex(r => r.CustomerId)
                .IsUnique();
        });

        modelBuilder.Entity<Merchant>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.HasIndex(m => m.UserId)
                .IsUnique();

            builder.HasIndex(m => m.Email)
                .IsUnique();

            builder.Property(m => m.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.LogoUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.OpeningHours)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.UnderratedScore)
                .HasPrecision(5, 2);

            builder.Property(m => m.Rating)
                .HasPrecision(3, 2);

            builder.Property(m => m.PlatformFeePercent)
                .HasPrecision(5, 2);

            builder.Property(m => m.Latitude)
                .HasPrecision(9, 6);

            builder.Property(m => m.Longitude)
                .HasPrecision(9, 6);
        });

        modelBuilder.Entity<Category>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(c => c.Slug)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(c => c.Slug)
                .IsUnique();

            builder.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CategoryDetail>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(cd => cd.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(cd => cd.ImgUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(cd => cd.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(cd => cd.Category)
                .WithMany(c => c.CategoryDetails)
                .HasForeignKey(cd => cd.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cd => cd.Food)
                .WithMany(f => f.CategoryDetails)
                .HasForeignKey(cd => cd.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(cd => new { cd.CategoryId, cd.FoodId })
                .IsUnique();
        });

        modelBuilder.Entity<Food>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(f => f.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(f => f.Price)
                .HasPrecision(18, 2);

            builder.Property(f => f.ImageUrl)
                .HasMaxLength(500);

            builder.HasOne(f => f.Merchant)
                .WithMany(m => m.Foods)
                .HasForeignKey(f => f.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Wishlist>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.HasIndex(w => w.CustomerId)
                .IsUnique();
        });

        modelBuilder.Entity<WishlistDetail>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.HasOne(wd => wd.Wishlist)
                .WithMany(w => w.WishlistDetails)
                .HasForeignKey(wd => wd.WishlistId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(wd => wd.Merchant)
                .WithMany(m => m.WishlistDetails)
                .HasForeignKey(wd => wd.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(wd => new { wd.WishlistId, wd.MerchantId })
                .IsUnique();
        });

        modelBuilder.Entity<CheckIn>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.HasOne(ci => ci.Customer)
                .WithMany(c => c.CheckIns)
                .HasForeignKey(ci => ci.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ci => ci.Merchant)
                .WithMany(m => m.CheckIns)
                .HasForeignKey(ci => ci.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(o => o.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(o => o.Discount)
                .HasPrecision(18, 2);

            builder.Property(o => o.FinalPrice)
                .HasPrecision(18, 2);

            builder.Property(o => o.ReviewerFee)
                .HasPrecision(18, 2);

            builder.Property(o => o.PlatformFee)
                .HasPrecision(18, 2);

            builder.Property(o => o.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(o => o.Notes)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(o => o.DeliveryAddress)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.AffiliateLink)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AffiliateLinkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderDetail>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(od => od.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            builder.Property(od => od.Notes)
                .HasMaxLength(1000);

            builder.HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(od => od.Food)
                .WithMany(f => f.OrderDetails)
                .HasForeignKey(od => od.FoodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Content)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(r => r.ImageUrl)
                .HasMaxLength(2048);

            builder.HasOne(r => r.Order)
                .WithOne(o => o.Review)
                .HasForeignKey<Review>(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(r => r.OrderId)
                .IsUnique();

            builder.HasOne(r => r.Merchant)
                .WithMany(m => m.Reviews)
                .HasForeignKey(r => r.MerchantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewDetail>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(rd => rd.DetailContent)
                .HasMaxLength(2000);

            builder.Property(rd => rd.Rating)
                .IsRequired();

            builder.HasOne(rd => rd.Review)
                .WithMany(r => r.ReviewDetails)
                .HasForeignKey(rd => rd.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rd => rd.OrderDetail)
                .WithOne(od => od.ReviewDetail)
                .HasForeignKey<ReviewDetail>(rd => rd.OrderDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(rd => rd.OrderDetailId)
                .IsUnique();
        });
    }

    public override int SaveChanges()
    {
        ApplyEntityRules();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyEntityRules();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityRules();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyEntityRules();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private static void ConfigureBaseEntity<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity<Guid>
    {
        builder.HasKey(x => x.Id);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }

    private void ApplyEntityRules()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is IAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.CreatedAt = now;
                    auditableEntity.UpdatedAt = null;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.UpdatedAt = now;
                }
            }

            if (entry.State == EntityState.Deleted && entry.Entity is BaseEntity<Guid> deletableEntity)
            {
                entry.State = EntityState.Modified;
                deletableEntity.IsDeleted = true;

                if (entry.Entity is IAuditableEntity deletedAuditableEntity)
                {
                    deletedAuditableEntity.UpdatedAt = now;
                }
            }
        }
    }
}
