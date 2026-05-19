using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Text.Json.Serialization;
using UGem.Api.Extensions;
using UGem.Api.Middlewares;
using UGem.Api.Options;
using UGem.Repositories;
using  UGem.Services.BackGroundJobService;
using FoodToppingService = UGem.Services.FoodToppingService;
using AffiliateLinkService = UGem.Services.AffiliateLinkService;
using ReviewerApplicationService = UGem.Services.ReviewerApplicationService;
using ReviewService = UGem.Services.ReviewService;
using UserService = UGem.Services.UserService;
using CheckInService = UGem.Services.CheckInService;
using WishlistService = UGem.Services.WishlistService;
using FoodService = UGem.Services.FoodService;
using MailService = UGem.Services.MailService;
using MediaService = UGem.Services.MediaService;
using CategoryService = UGem.Services.Category;
using CloudinaryService = UGem.Services.CloudinaryService;
using ApplicationService = UGem.Services.Application;
using NotificationService = UGem.Services.NotificationService;
using OrderService = UGem.Services.OrderService;
using IdentityService = UGem.Services.IdentityService;
using MonetizationService = UGem.Services.MonetizationService;
using JwtService = UGem.Services.JwtService;
using CustomerService = UGem.Services.CustomerService;
using MerchantService = UGem.Services.MerchantService;
using StaffService = UGem.Services.StaffService;
using AdminService = UGem.Services.AdminService;
using MailServiceOptions = UGem.Services.MailService.MailOption.MailOptions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigureValidatedOptions(builder.Services, builder.Configuration, builder.Environment);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"];

if (!builder.Environment.IsDevelopment() && HasPlaceholder(connectionString))
{
    throw new InvalidOperationException("A secure database connection string must be configured before startup.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        o => o.UseNetTopologySuite()
    )
);

builder.Services.AddJwtServices(builder.Configuration);
builder.Services.AddSwaggerServices();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<MailService.IService, MailService.Service>();
builder.Services.AddScoped<MediaService.IService, CloudinaryService.Service>();
builder.Services.AddScoped<CategoryService.IService, CategoryService.Service>();
builder.Services.AddScoped<ApplicationService.IService, ApplicationService.Service>();
builder.Services.AddScoped<NotificationService.IService, NotificationService.Service>();
builder.Services.AddScoped<OrderService.IService, OrderService.Service>();
builder.Services.AddScoped<IdentityService.IService, IdentityService.Service>();
builder.Services.AddScoped<JwtService.IService, JwtService.Service>();
builder.Services.AddScoped<CustomerService.IService, CustomerService.Service>();
builder.Services.AddScoped<MerchantService.IService, MerchantService.Service>();
builder.Services.AddScoped<FoodService.IService, FoodService.Service>();
builder.Services.AddScoped<WishlistService.IService, WishlistService.Service>();
builder.Services.AddScoped<CheckInService.IService, CheckInService.Service>();
builder.Services.AddScoped<UserService.IService, UserService.Service>();
builder.Services.AddScoped<ReviewService.IService, ReviewService.Service>();
builder.Services.AddScoped<AdminService.IService, AdminService.Service>();
builder.Services.AddScoped<AffiliateLinkService.IService, AffiliateLinkService.Service>();
builder.Services.AddScoped<ReviewerApplicationService.IService, ReviewerApplicationService.Service>();
builder.Services.AddScoped<StaffService.IService, StaffService.Service>();
builder.Services.AddScoped<MonetizationService.IService, MonetizationService.Service>();
builder.Services.AddScoped<FoodToppingService.IService, FoodToppingService.Service>();
builder.Services.AddScoped<RebalancingJob>();
builder.Services.AddQuartz(options =>
{
    var rebalancingJobKey = new JobKey(nameof(RebalancingJob));
    options.AddJob<RebalancingJob>(rebalancingJobKey)
        .AddTrigger(trigger => trigger
            .ForJob(rebalancingJobKey)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInHours(24*3)
                .RepeatForever())
            .StartNow()); 
    
    var processTransactionPendingJob = new JobKey(nameof(ProcessTransactionPendingJob));
    options.AddJob<ProcessTransactionPendingJob>(processTransactionPendingJob)
        .AddTrigger(trigger => trigger
            .ForJob(processTransactionPendingJob)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInMinutes(1)
                .RepeatForever())
            .StartNow());
});
builder.Services.AddCors(options =>
{
    var corsOptions = builder.Configuration
        .GetSection(CorsOptions.SectionName)
        .Get<CorsOptions>() ?? new CorsOptions();

    options.AddPolicy("AllowFrontend", policy =>
    {
        if (corsOptions.AllowedOrigins.Length == 0)
        {
            return;
        }

        policy
            .WithOrigins(corsOptions.AllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await EnsureOrderCustomerNullableAsync(dbContext);
}

// Configure the HTTP request pipeline.

var enableSwagger = builder.Configuration.GetValue<bool?>("Features:EnableSwagger")
    ?? builder.Environment.IsDevelopment();

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void ConfigureValidatedOptions(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
{
    services.AddOptions<JwtService.JwtOptions>()
        .Bind(configuration.GetSection(nameof(JwtService.JwtOptions)));

    services.AddOptions<CloudinaryService.CloudinaryOptions>()
        .Bind(configuration.GetSection(nameof(CloudinaryService.CloudinaryOptions)));

    services.Configure<MailServiceOptions>(
        configuration.GetSection("MailOptions"));

    if (environment.IsDevelopment())
    {
        return;
    }

    services.AddOptions<MailServiceOptions>()
        .ValidateDataAnnotations()
        .Validate(options =>
                HasConfiguredValue(options.Mail)
                && HasConfiguredValue(options.DisplayName)
                && HasConfiguredValue(options.Password)
                && HasConfiguredValue(options.Host)
                && options.Port > 0,
            "MailOptions must be configured with secure non-placeholder values.")
        .ValidateOnStart();

    services.AddOptions<JwtService.JwtOptions>()
        .ValidateDataAnnotations()
        .Validate(options =>
                HasConfiguredValue(options.SecretKey)
                && HasConfiguredValue(options.Issuer)
                && HasConfiguredValue(options.Audience)
                && options.ExpireMinutes > 0,
            "JwtOptions must be configured with secure non-placeholder values.")
        .ValidateOnStart();

    services.AddOptions<CloudinaryService.CloudinaryOptions>()
        .ValidateDataAnnotations()
        .Validate(options =>
                HasConfiguredValue(options.CloudName)
                && HasConfiguredValue(options.ApiKey)
                && HasConfiguredValue(options.ApiSecret),
            "CloudinaryOptions must be configured with secure non-placeholder values.")
        .ValidateOnStart();
}

static bool HasConfiguredValue(string? value)
{
    return !string.IsNullOrWhiteSpace(value) && !HasPlaceholder(value);
}

static bool HasPlaceholder(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return true;
    }

    return value.Contains("__SET", StringComparison.OrdinalIgnoreCase)
           || value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase)
           || value.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase);
}

static async Task EnsureOrderCustomerNullableAsync(AppDbContext dbContext)
{
    const string sql = """
        DO $$
        DECLARE
            orders_reg regclass := to_regclass('public."Orders"');
            customers_reg regclass := to_regclass('public."Customers"');
            customer_id_attnum smallint;
            fk_name text;
        BEGIN
            IF orders_reg IS NULL OR customers_reg IS NULL THEN
                RETURN;
            END IF;

            SELECT attnum
            INTO customer_id_attnum
            FROM pg_attribute
            WHERE attrelid = orders_reg
              AND attname = 'CustomerId'
              AND NOT attisdropped;

            IF customer_id_attnum IS NULL THEN
                RETURN;
            END IF;

            SELECT conname
            INTO fk_name
            FROM pg_constraint
            WHERE conrelid = orders_reg
              AND contype = 'f'
              AND customer_id_attnum = ANY (conkey)
            LIMIT 1;

            IF fk_name IS NOT NULL THEN
                EXECUTE format('ALTER TABLE %s DROP CONSTRAINT %I', orders_reg, fk_name);
            END IF;

            EXECUTE format(
                'UPDATE %s SET "CustomerId" = NULL WHERE "CustomerId" = %L::uuid',
                orders_reg,
                '00000000-0000-0000-0000-000000000000'
            );

            EXECUTE format('ALTER TABLE %s ALTER COLUMN "CustomerId" DROP NOT NULL', orders_reg);

            IF NOT EXISTS (
                SELECT 1
                FROM pg_constraint
                WHERE conrelid = orders_reg
                  AND conname = 'FK_Orders_Customers_CustomerId'
            ) THEN
                EXECUTE format(
                    'ALTER TABLE %s ADD CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES %s ("Id") ON DELETE SET NULL',
                    orders_reg,
                    customers_reg
                );
            END IF;
        END $$;
        """;

    await dbContext.Database.ExecuteSqlRawAsync(sql);
}