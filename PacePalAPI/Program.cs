using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PacePalAPI.Controllers.Middleware;
using PacePalAPI.Models;
using PacePalAPI.Services.AlertService;
using PacePalAPI.Services.SocialService;
using PacePalAPI.Services.TrackService;
using PacePalAPI.Services.UserSearchService;
using PacePalAPI.Services.UserSearchService.Impl;
using PacePalAPI.Services.UserService;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Jwt configuration starts here
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
     };
 });

// Add services to the container.
// Register RadixTree as Singleton
builder.Services.AddSingleton<IUserSearchService, UserSearchService>(provider =>
{
    // Resolve the database context
    using var scope = provider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PacePalContext>();

    // Retrieve the list of users from the database
    var users = dbContext.Users
        .Include(u => u.SentFriendships)
        .Include(u => u.ReceivedFriendships)
        .ToList();

    // Retrieve or initialize Radix Trees (if not registered as singletons separately)
    var usernamesRadixTree = new RadixTree();
    var firstnameRadixTree = new RadixTree();
    var lastnameRadixTree = new RadixTree();
    var fullNameRadixTree = new RadixTree();

    // Pass all dependencies to the UserSearchService
    return new UserSearchService(
        usernamesRadixTree,
        firstnameRadixTree,
        lastnameRadixTree,
        fullNameRadixTree,
        users);
});

builder.Services.AddScoped<IUserCollectionService, UserService>();
builder.Services.AddScoped<ISocialPostCollectionService, SocialPostService>();
builder.Services.AddScoped<ITrackCollectionService, TrackService>();
builder.Services.AddScoped<IAlertCollectionService, AlertService>();


builder.Services.AddSingleton<MyWebSocketManager>();

// Configure session services
builder.Services.AddDistributedMemoryCache(); // In-memory cache for session storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120); // Session timeout
    options.Cookie.HttpOnly = true; // Enhance security by making the session cookie HttpOnly
    options.Cookie.IsEssential = true; // Ensure the session cookie is always available
});

builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

// Manually set the WebRootPath to ensure it's not null
builder.Environment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "HikeAidAPI", Version = "v1" });
    options.OperationFilter<ImageResponseOperationFilter>();
});

builder.Services.AddDbContext<PacePalContext>(options =>
{
    options.UseSqlServer("Server=DESKTOP-RTM4QH7\\SQLEXPRESS;Database=PacePalDb;TrustServerCertificate=True;Integrated Security=True;");
});

builder.Services.AddDbContextFactory<PacePalContext>(options =>
    options.UseSqlServer("Server=DESKTOP-RTM4QH7\\SQLEXPRESS;Database=PacePalDb;TrustServerCertificate=True;Integrated Security=True;"), ServiceLifetime.Scoped);

var app = builder.Build();

// Ensure UserSearchService is instantiated at startup
using (var scope = app.Services.CreateScope())
{
    var userSearchService = scope.ServiceProvider.GetRequiredService<IUserSearchService>();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles(); // Enable serving static files from wwwroot

app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Add session middleware

app.UseWebSockets(); // Enable WebSockets
app.UseMiddleware<WebSocketMiddleware>(); // Add WebSocket middleware

app.MapControllers();

app.Run();
