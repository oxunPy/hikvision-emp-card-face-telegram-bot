using CardManagement;
using FaceManagement;
using hikvision_emp_card_face_telegram_bot;
using hikvision_emp_card_face_telegram_bot.bot;
using hikvision_emp_card_face_telegram_bot.bot.ActionHandler;
using hikvision_emp_card_face_telegram_bot.Bot;
using hikvision_emp_card_face_telegram_bot.Data;
using hikvision_emp_card_face_telegram_bot.Interfaces;
using hikvision_emp_card_face_telegram_bot.Repository;
using hikvision_emp_card_face_telegram_bot.Service;
using hikvision_emp_card_face_telegram_bot.Service.Impl;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Telegram.Bot;

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
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<ILunchMenuRepository, LunchMenuRepository>();
builder.Services.AddScoped<ISelectedMenuRepository, SelectedMenuRepository>();
builder.Services.AddScoped<ITerminalConfigurationRepository, TerminalConfigurationRepository>();

// services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDishService, DishService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ILunchMenuService, LunchMenuService>();
builder.Services.AddScoped<ISelectedMenuService, SelectedMenuService>();
builder.Services.AddScoped<ITerminalConfigurationService, TerminalConfigurationService>();

// Register the bot
builder.Services.AddSingleton<TelegramBotClient>(provider =>
{ 
    string botToken = builder.Configuration["TelegramBot:BotToken"];
    return new TelegramBotClient(botToken);
});
builder.Services.AddSingleton<TelegramBotService>();
builder.Services.AddSingleton<CallbackHandler>();
builder.Services.AddSingleton<MessageHandler>();
builder.Services.AddSingleton<RegisterHandler>();

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
