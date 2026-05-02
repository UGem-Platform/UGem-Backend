using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Text.Json.Serialization;
using UGem.Api.Extensions;
using UGem.Repositories;
using UGem.Services.BackGroundJobService;
using QRCodeService = UGem.Services.QRCodeService;
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

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"];

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
builder.Services.AddScoped<QRCodeService.IService, QRCodeService.Service>();
builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey(nameof(ProcessTransactionPendingJob));

    options
        .AddJob<ProcessTransactionPendingJob>(jobKey)
        .AddTrigger(trigger =>
            trigger
                .ForJob(jobKey)
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(2)
                    .RepeatForever()
                )
        );
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3001",
                "https://stimulate-gutter-sliceable.ngrok-free.dev", // your ngrok URL
                "https://u-gem-eight.vercel.app" // your production URL
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // IMPORTANT for SignalR
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

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseExceptionHandler("/error");

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();