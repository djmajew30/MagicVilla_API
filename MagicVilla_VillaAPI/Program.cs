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
