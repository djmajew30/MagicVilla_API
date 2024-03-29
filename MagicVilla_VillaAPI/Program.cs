//using Serilog;

using MagicVilla_VillaAPI.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//added in 27. patch nuget packages
//builder.Services.AddControllers().AddNewtonsoftJson();
//30. content negotiations
builder.Services.AddControllers(option => {
    //option.ReturnHttpNotAcceptable = true; // commented out in 31 so swagger still works with plain text
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

////32. serilog to log to file
//Log.Logger = new LoggerConfiguration().MinimumLevel.Debug() //use info or error for less
//    .WriteTo.File("log/villaLogs.txt",rollingInterval:RollingInterval.Day).CreateLogger();

////to tell program to use serilog instead on built in logger:
//builder.Host.UseSerilog();


//33. custom logger instead of default logger
//builder.Services.AddSingleton<ILogging, Logging>();
//builder.Services.AddSingleton<ILogging, LoggingV2>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
