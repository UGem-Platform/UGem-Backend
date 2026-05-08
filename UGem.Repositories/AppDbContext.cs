using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using UGem.Repositories.Abtraction;
using UGem.Repositories.Entity;

namespace UGem.Repositories;

public class AppDbContext : DbContext
{
    private static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AdminProfileId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid UserStaffId1 = Guid.Parse("19191919-1919-1919-1919-191919191919");
    private static readonly Guid UserCustomerId2 = Guid.Parse("20202020-2020-2020-2020-202020202020");
    private static readonly Guid StaffProfileId1 = Guid.Parse("21212121-2121-2121-2121-212121212121");
    private static readonly Guid CustomerProfileId1 = Guid.Parse("23232323-2323-2323-2323-232323232323");
    private static readonly Guid StaffUserId2 = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid StaffProfileId2 = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid ApplicantUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    private static readonly Guid ApplicantCustomerId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly Guid MerchantUserId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid MerchantProfileId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    private static readonly Guid DiscoveryUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    private static readonly Guid DiscoveryCustomerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid PendingApplicationId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid ApplicationMenuId1 = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid ApplicationMenuId2 = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    private static readonly Guid CategoryStreetFoodId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee");
    private static readonly Guid CategoryTraditionalFoodId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
    private static readonly Guid FoodId1 = Guid.Parse("12121212-1212-1212-1212-121212121212");
    private static readonly Guid FoodId2 = Guid.Parse("13131313-1313-1313-1313-131313131313");
    private static readonly Guid CategoryDetailId1 = Guid.Parse("14141414-1414-1414-1414-141414141414");
    private static readonly Guid CategoryDetailId2 = Guid.Parse("15151515-1515-1515-1515-151515151515");
    private static readonly Guid WishlistId1 = Guid.Parse("16161616-1616-1616-1616-161616161616");
    private static readonly Guid WishlistDetailId1 = Guid.Parse("17171717-1717-1717-1717-171717171717");
    private static readonly Guid CheckInId1 = Guid.Parse("18181818-1818-1818-1818-181818181818");
    private static readonly Guid ReviewerApplicationId1 = Guid.Parse("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"); 

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<ApplicationMenu> ApplicationMenus { get; set; }
    public DbSet<Notification> Notifications { get; set; }
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
    public DbSet<ReviewerApplication> ReviewerApplications { get; set; } 
    public DbSet<ReviewDetail> ReviewDetails { get; set; }


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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "0123456789",
                IsActive = true,
                Role = "Admin",
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new User
            {
                Id = UserCustomerId2,
                Email = "hungsui@gmail.com",
                FullName = "Trần Văn Hùng",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "902222222",
                IsActive = true,
                Role = "Customer",
                CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new User
            {
                Id = UserStaffId1,
                Email = "staff.ngoc@ugem.com",
                FullName = "Lê Bảo Ngọc",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "901111111",
                IsActive = true,
                Role = "Staff",
                CreatedAt = new DateTimeOffset(2026, 4, 21, 8, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new User
            {
                Id = StaffUserId2,
                Email = "staff.duyet@ugem.com",
                FullName = "Nguyen Tran Admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "0901000001",
                IsActive = true,
                Role = "Staff",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new User
            {
                Id = ApplicantUserId,
                Email = "chuquan.bun@ugem.com",
                FullName = "Tran Bun Cha",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "0902000002",
                IsActive = true,
                Role = "Customer",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new User
            {
                Id = MerchantUserId,
                Email = "chuquan.che@ugem.com",
                FullName = "Le Thi Che",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "0902000003",
                IsActive = true,
                Role = "Merchant",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new User
            {
                Id = DiscoveryUserId,
                Email = "khach.sanh@ugem.com",
                FullName = "Pham Thuc Khach",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                PhoneNumber = "0903000004",
                IsActive = true,
                Role = "Customer",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
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
            var staff = new List<Staff>()
            {
                new()
                {
                    Id = StaffProfileId1,
                    UserId = UserStaffId1,
                    HiredAt = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero),
                    CreatedAt = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero),
                    IsDeleted = false
                },
                new()
                {
                    Id = StaffProfileId2,
                    UserId = StaffUserId2,
                    HiredAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                    IsDeleted = false
                }
            };
            builder.HasData(staff);
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

        modelBuilder.Entity<Application>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(a => a.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.Note)
                .HasMaxLength(2000);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(a => a.RestaurantType)
                .HasMaxLength(200);

            builder.Property(a => a.MainDishType)
                .HasMaxLength(200);

            builder.Property(a => a.PriceRange)
                .HasMaxLength(200);

            builder.Property(a => a.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(a => a.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(a => a.LogoUrl)
                .IsRequired()
                .HasMaxLength(500);
            
            builder.Property(a => a.OpeningHours)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(a => a.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.Latitude)
                .HasPrecision(9, 6);

            builder.Property(a => a.Longitude)
                .HasPrecision(9, 6);

            builder.HasOne(a => a.User)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Application>().HasData(
            new Application
            {
                Id = PendingApplicationId,
                UserId = ApplicantUserId,
                Type = "Merchant",
                Status = "Pending",
                Note = null,
                ReviewedAt = new DateTime(2026, 4, 23, 8, 0, 0, DateTimeKind.Utc),
                Name = "Bun Cha Ngo Tram",
                Description = "Nam sau trong ngo, thit nuong kep que tre truyen thong 30 nam.",
                Email = "lienhe@bunchangotram.vn",
                Phone = "0911223344",
                LogoUrl = "logo_buncha.jpg",
                Address = "Lau 1, 123 Ngo Tram, Q1",
                OpeningHours = "08:00 - 22:00",
                Latitude = 21.030500m,
                Longitude = 105.845600m,
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

        modelBuilder.Entity<ApplicationMenu>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(am => am.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(am => am.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(am => am.Price)
                .HasPrecision(18, 2);

            builder.Property(am => am.ImageUrl)
                .HasMaxLength(500);

            builder.Property(am => am.Category)
                .HasMaxLength(100);

            builder.HasOne(am => am.Application)
                .WithMany(a => a.ApplicationMenus)
                .HasForeignKey(am => am.ApplicationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ApplicationMenu>().HasData(
            new ApplicationMenu
            {
                Id = ApplicationMenuId1,
                ApplicationId = PendingApplicationId,
                Name = "Bun cha thap cam",
                Description = "Day du cha bam, cha mieng, cha la lot nuong xiem.",
                Price = 45000m,
                Category = "Mon chinh",
                ImageUrl = "img_buncha_1.jpg",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 5, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new ApplicationMenu
            {
                Id = ApplicationMenuId2,
                ApplicationId = PendingApplicationId,
                Name = "Nem cua be",
                Description = "Nem vuong, vo gion rum, nhan cua bien tuoi.",
                Price = 25000m,
                Category = "An kem",
                ImageUrl = "img_nemcua.jpg",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 5, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Notification>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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

            builder.HasData(
                new Customer
                {
                    Id = CustomerProfileId1,
                    UserId = UserCustomerId2,
                    TotalCheckIns = 5,
                    CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    IsDeleted = false
                },
                new Customer
                {
                    Id = ApplicantCustomerId,
                    UserId = ApplicantUserId,
                    TotalCheckIns = 0,
                    CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                    IsDeleted = false
                },
                new Customer
                {
                    Id = DiscoveryCustomerId,
                    UserId = DiscoveryUserId,
                    TotalCheckIns = 1,
                    CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 0, 0, TimeSpan.Zero),
                    IsDeleted = false
                }
            );
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

            builder.Property(m => m.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(m => m.RestaurantType)
                .HasMaxLength(200);

            builder.Property(m => m.MainDishType)
                .HasMaxLength(200);

            builder.Property(m => m.PriceRange)
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

            builder.Property(m => m.Location)
                .IsRequired()
                .HasColumnType("geometry(Point, 4326)");

            builder.HasIndex(m => new { m.Name, m.Description });
            
            builder.HasIndex(m => m.Location)
                .HasMethod("GIST");
        });

        modelBuilder.Entity<Merchant>().HasData(
            new Merchant
            {
                Id = MerchantProfileId,
                UserId = MerchantUserId,
                Name = "Che Mam Cau Hai",
                Description = "Quan che 16 mon an minh trong chung cu cu, khong bien hieu.",
                Email = "checauhai@ugem.com",
                Phone = "0909888777",
                Address = "Lau 2 Chung cu Ton That Dam, Q1",
                IsActive = true,
                LogoUrl = "logo_che.png",
                Status = "Active",
                UnderratedScore = 4.90m,
                Rating = 4.80m,
                PlatformFeePercent = 5.00m,
                OpeningHours = "15:00 - 22:00",
                Latitude = 10.771500,
                Longitude = 106.703200,
                Location = new Point(106.703200, 10.771500) { SRID = 4326 },
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 15, 0, TimeSpan.Zero),
                IsDeleted = false,
                PriceRange = 5000
            }
        );

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

            builder.Property(c => c.Path)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasIndex(c => c.Slug)
                .IsUnique();

            builder.HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = CategoryStreetFoodId,
                Name = "An Vat Le Duong",
                Description = "Cac mon an vat moc mac, gia hoc sinh sinh vien.",
                Slug = "an-vat-le-duong",
                Path = "/an-vat-le-duong",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 10, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new Category
            {
                Id = CategoryTraditionalFoodId,
                Name = "Mon Nuoc Truyen Thong",
                Description = "Pho, bun, hu tieu voi nuoc dung ninh ham lau nam.",
                Slug = "mon-nuoc-truyen-thong",
                Path = "/mon-nuoc-truyen-thong",
                IsActive = true,
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 10, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

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

        modelBuilder.Entity<CategoryDetail>().HasData(
            new CategoryDetail
            {
                Id = CategoryDetailId1,
                CategoryId = CategoryStreetFoodId,
                FoodId = FoodId1,
                Name = "Che Ba Ba Nam Bo",
                ImgUrl = "img_che_baba.jpg",
                Description = "Mon ngot dac trung mien Tay Nam Bo.",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 25, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new CategoryDetail
            {
                Id = CategoryDetailId2,
                CategoryId = CategoryStreetFoodId,
                FoodId = FoodId2,
                Name = "Trang Mieng",
                ImgUrl = "img_flan_cotdua.jpg",
                Description = "Phu hop an nhe buoi chieu.",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 25, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

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
            
            builder.HasIndex(f => new { f.Name, f.Description });
        });

        modelBuilder.Entity<Food>().HasData(
            new Food
            {
                Id = FoodId1,
                MerchantId = MerchantProfileId,
                Name = "Che Ba Ba",
                Description = "Khoai lang, khoai mi, chuoi chung cot dua beo ngay.",
                Price = 20000m,
                IsAvailable = true,
                ImageUrl = "img_che_baba.jpg",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 20, 0, TimeSpan.Zero),
                IsDeleted = false
            },
            new Food
            {
                Id = FoodId2,
                MerchantId = MerchantProfileId,
                Name = "Banh Flan Cot Dua",
                Description = "Banh flan mem min an kem ca phe den va cot dua.",
                Price = 15000m,
                IsAvailable = true,
                ImageUrl = "img_flan_cotdua.jpg",
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 20, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

        modelBuilder.Entity<Wishlist>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.HasIndex(w => w.CustomerId)
                .IsUnique();
        });

        modelBuilder.Entity<Wishlist>().HasData(
            new Wishlist
            {
                Id = WishlistId1,
                CustomerId = DiscoveryCustomerId,
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 30, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

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

        modelBuilder.Entity<WishlistDetail>().HasData(
            new WishlistDetail
            {
                Id = WishlistDetailId1,
                WishlistId = WishlistId1,
                MerchantId = MerchantProfileId,
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 31, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

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

        modelBuilder.Entity<CheckIn>().HasData(
            new CheckIn
            {
                Id = CheckInId1,
                MerchantId = MerchantProfileId,
                CustomerId = DiscoveryCustomerId,
                CreatedAt = new DateTimeOffset(2026, 4, 23, 8, 32, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

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
            
            builder.Property(o => o.RejectionReason)
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
        
        modelBuilder.Entity<ReviewerApplication>(builder =>
        {
            ConfigureBaseEntity(builder);

            builder.Property(ra => ra.Status)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ra => ra.Motivation)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(ra => ra.Experience)
                .HasMaxLength(2000);

            builder.Property(ra => ra.FacebookUrl)
                .HasMaxLength(500);

            builder.Property(ra => ra.TiktokUrl)
                .HasMaxLength(500);

            builder.Property(ra => ra.YoutubeUrl)
                .HasMaxLength(500);

            builder.Property(ra => ra.OtherSocialUrl)
                .HasMaxLength(500);

            builder.HasOne(ra => ra.Customer)
                .WithMany(c => c.ReviewerApplications)
                .HasForeignKey(ra => ra.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReviewerApplication>().HasData(
            new ReviewerApplication
            {
                Id = ReviewerApplicationId1,
                CustomerId = ApplicantCustomerId,
                Status = "Pending",
                Motivation = "Toi co nieu tu long yeu am thuc duong pho va muon chia se trai nghiem that su den cong dong.",
                Experience = "2 nam viet blog am thuc ca nhan, 5000+ followers tren Facebook.",
                FacebookUrl = "https://facebook.com/applicant.reviewer",
                TiktokUrl = "https://tiktok.com/@applicant.reviewer",
                YoutubeUrl = null,
                OtherSocialUrl = null,
                CreatedAt = new DateTimeOffset(2026, 4, 24, 9, 0, 0, TimeSpan.Zero),
                IsDeleted = false
            }
        );

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

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
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
