using CardManagement;
using FaceManagement;
using hikvision_emp_card_face_telegram_bot;
using hikvision_emp_card_face_telegram_bot.bot;
using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using hikvision_emp_card_face_telegram_bot.Repository;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//builder.Services.AddTransient<Seed>();

builder.Services.AddControllers().AddJsonOptions(x =>
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// repositories
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IDishRepository, DishRepository>();

// Register the TelegramService
builder.Services.AddSingleton<TelegramBotService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"));
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Start the bot service in the background 
var telegramBotService = app.Services.GetRequiredService<TelegramBotService>();
var cancellationToken = app.Lifetime.ApplicationStopping;
await telegramBotService.StartBotAsync(cancellationToken);

// Initialize HCNetSDK
var currentDir = AppDomain.CurrentDomain.BaseDirectory;
Console.WriteLine(currentDir);

if (CHCNetSDKForCard.NET_DVR_Init() == false)
{
    Console.WriteLine("NET_DVR_Init error!");
    return;
}

if(CHCNetSDKForFace.NET_DVR_Init() == false)
{
    Console.WriteLine("NET_DVR_Init error!");
    return;
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
