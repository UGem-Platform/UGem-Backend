using Microsoft.EntityFrameworkCore;
using Quartz;
using UGem.Api.Extensions;
using UGem.Repositories;
using UGem.Services.BackGroundJobService;
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

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
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
builder.Services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();