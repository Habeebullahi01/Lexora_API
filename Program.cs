using System.Text;
using lexora_api.Data;
using lexora_api.Middlewares;
using lexora_api.Models;
using lexora_api.Services;
using lexora_api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();
var url = System.Environment.GetEnvironmentVariable("DB_URL")!;
string host = System.Environment.GetEnvironmentVariable("HOST")!;
string user = System.Environment.GetEnvironmentVariable("USER")!;
string pwd = System.Environment.GetEnvironmentVariable("PASSWORD")!;
string dbname = System.Environment.GetEnvironmentVariable("DBNAME")!;
var port = Environment.GetEnvironmentVariable("PORT")!;

string u = $"Host={host};PORT={port};Username={user};Password={pwd};Database={dbname}";


// Console.WriteLine(u);
// Console.WriteLine(typeof(url));


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title = "Lexora Library Management Service Documentation",
            Description = "This is the documentation for Lexora.",
            Version = "0.1"
        };
        return Task.CompletedTask;
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

// Serilog
// Log.Logger = new LoggerConfiguration().WriteTo.File("testLog.txt").CreateLogger();
// builder.Services.AddSerilog();
// Add DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(u);
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(u);
});

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();
// Configure Identity
builder.Services.Configure<IdentityOptions>(op =>
{
    op.User.RequireUniqueEmail = true;
});
// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Add IBookService
builder.Services.AddScoped<IBookService, BookService>();
//Add IRequestService
builder.Services.AddScoped<IRequestService, RequestService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // policy.WithOrigins("https://localhost:7256") // Allow specific origins
        policy.AllowAnyOrigin() // Allow specific origins
              .AllowAnyHeader()
              .AllowAnyMethod();
        //   .AllowCredentials(); // Optional, if you're using cookies or auth headers
    });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    // var logger = new 
    await DbIintializer.SeedRoles(services);
}

// app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Default");
    options.RoutePrefix = "docs";
    options.DocumentTitle = "Lexora";
});

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCus();
app.MapControllers();

app.Run();
