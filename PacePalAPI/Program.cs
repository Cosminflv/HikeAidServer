using Microsoft.EntityFrameworkCore;
using PacePalAPI.Models;
using PacePalAPI.Services.UserService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserCollectionService, UserService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PacePalContext>(options =>
{
    options.UseSqlServer("Server=DESKTOP-RTM4QH7\\SQLEXPRESS;Database=PacePalDb;TrustServerCertificate=True;Integrated Security=True;");
});

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
