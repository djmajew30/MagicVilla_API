//using Serilog;

using MagicVilla_VillaAPI;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Repository.IRepostiory;
using MagicVilla_VillaAPI.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//38. use connections string
builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

//50. repository
builder.Services.AddScoped<IVillaRepository, VillaRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();

//46 automapper
builder.Services.AddAutoMapper(typeof(MappingConfig));

//93 jwt authentication
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x => {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    }); ;

//added in 27. patch nuget packages
//builder.Services.AddControllers().AddNewtonsoftJson();

//30. content negotiations
builder.Services.AddControllers(option =>
{
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

//93 jwt authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
