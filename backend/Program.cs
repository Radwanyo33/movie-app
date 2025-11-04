using Microsoft.EntityFrameworkCore;
using Live_Movies.Data;
using Live_Movies.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(8080);
    });
    
    // Also keep the UseUrls as backup
    builder.WebHost.UseUrls("http://0.0.0.0:8080");
}


// Add session services
builder.Services.AddDistributedMemoryCache(); // Or use Redis for production
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(2); // 2 days session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "LMDB.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Use Always in production
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration

// Database Configuration
if (builder.Environment.IsProduction())
{
    // Use PostgreSQL in production
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Validate connection string exists
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Production database connection string is not configured.");
    }
    
    builder.Services.AddDbContext<MovieDbContext>(options =>
        options.UseNpgsql(connectionString, npgOptions =>
        {
            npgOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null
            );
        }));
}
else
{
    // Use SQL Server in development (with your existing retry configuration)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<MovieDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
}

    

// Register Services
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IImageService, ImageService>();

// CORS Configuration
// For production, use environment variables
var allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] { "http://localhost:5173", "http://localhost:3000" }
    : new[] { "https://livemovies.onrender.com" }; // Added Render frontend URL

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Logging
builder.Services.AddLogging();


var app = builder.Build();

// Ensure uploads directory exists for development
if (app.Environment.IsDevelopment())
{
    var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads", "movies");
    if (!Directory.Exists(uploadsPath))
    {
        Directory.CreateDirectory(uploadsPath);
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    // Production settings
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// CORS must be before UseAuthorization and MapControllers
app.UseCors("AllowReactApp");

// Static files configuration
app.UseStaticFiles(); // This serves wwwroot folder

// Static files for uploads directory - for production
try 
{
    var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
    if (Directory.Exists(uploadsPath) || app.Environment.IsDevelopment())
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadsPath),
            RequestPath = "/uploads",
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
            }
        });
    }
}
catch (Exception ex)
{
    var logger = app.Logger;
    logger.LogWarning(ex, "Failed to configure uploads static files");
}

app.UseSession();
app.UseAuthorization();
app.MapControllers();

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MovieDbContext>();
        
        // Try to apply migrations
        try 
        {
            await context.Database.MigrateAsync();
            Console.WriteLine("Database migrations applied successfully.");
        }
        catch (Exception migrateEx)
        {
            // If migrations fail, try to ensure database is created
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(migrateEx, "Database migration failed, trying to ensure database is created...");
            
            // Force database creation
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database ensured created (tables may not match model).");
        }
        
        // Try to seed data - continue even if it fails
        try 
        {
            await SeedData.Initialize(services);  // Keep this commented for now
            Console.WriteLine("Database seeding Completed.");
        }
        catch (Exception seedEx)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogWarning(seedEx, "Database seeding failed, but continuing application startup");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An unexpected error occurred during database setup");
    }
}

app.Run();