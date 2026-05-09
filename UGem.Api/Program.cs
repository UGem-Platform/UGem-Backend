using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Text.Json.Serialization;
using UGem.Api.Extensions;
using UGem.Api.Middlewares;
using UGem.Api.Options;
using UGem.Repositories;
using  UGem.Services.BackGroundJobService;
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
using JwtService = UGem.Services.JwtService;
using CustomerService = UGem.Services.CustomerService;
using MerchantService = UGem.Services.MerchantService;
using StaffService = UGem.Services.StaffService;
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
builder.Services.AddScoped<ReviewerApplicationService.IService, ReviewerApplicationService.Service>();
builder.Services.AddScoped<StaffService.IService, StaffService.Service>();
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
        .Configure(options =>
        {
            options.CloudName = "df1gdohe5";
            options.ApiKey = "559921934694338";
            options.ApiSecret = "x5LaRO61tpsNFbRkrKGJLpPUMp4";
        });

    services.AddOptions<MailServiceOptions>()
        .Bind(configuration.GetSection(nameof(MailServiceOptions)));

    if (environment.IsDevelopment())
    {
        return;
    }

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
