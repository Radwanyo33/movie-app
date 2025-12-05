using Microsoft.EntityFrameworkCore;
using Live_Movies.Data;
using Live_Movies.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel for Railway
if (builder.Environment.IsProduction())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.ListenAnyIP(8080);
    });
    
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
    options.Cookie.SameSite = SameSiteMode.None; // Required for cross-origin in production
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
if (builder.Environment.IsProduction())
{
    // Use PostgreSQL in production (Railway provides these env vars)
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Fallback for Railway's default PostgreSQL connection
    if (string.IsNullOrEmpty(connectionString))
    {
        // Railway provides these environment variables
        var pgHost = Environment.GetEnvironmentVariable("PGHOST");
        var pgPort = Environment.GetEnvironmentVariable("PGPORT");
        var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");
        var pgUser = Environment.GetEnvironmentVariable("PGUSER");
        var pgPassword = Environment.GetEnvironmentVariable("PGPASSWORD");
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            // Parse DATABASE_URL format: postgresql://user:pass@host:port/db
            connectionString = databaseUrl.Replace("postgresql://", "");
            var parts = connectionString.Split('@');
            if (parts.Length == 2)
            {
                var credentials = parts[0].Split(':');
                var hostAndDb = parts[1].Split('/');
                var hostAndPort = hostAndDb[0].Split(':');
                
                connectionString = $"Host={hostAndPort[0]};Port={hostAndPort[1]};Database={hostAndDb[1]};Username={credentials[0]};Password={credentials[1]};SSL Mode=Require;Trust Server Certificate=true";
            }
        }
        else if (!string.IsNullOrEmpty(pgHost))
        {
            connectionString = $"Host={pgHost};Port={pgPort};Database={pgDatabase};Username={pgUser};Password={pgPassword};SSL Mode=Require;Trust Server Certificate=true";
        }
    }
    
    builder.Services.AddDbContext<MovieDbContext>(options =>
        options.UseNpgsql(connectionString, npgOptions =>
        {
            npgOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorCodesToAdd: null
            );
            npgOptions.SetPostgresVersion(new Version(13, 0)); // Railway uses PostgreSQL 13+
        }));
}
else
{
    // Use SQL Server in development
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

// CORS Configuration for Railway
var allowedOrigins = builder.Environment.IsDevelopment()
    ? new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:8080" }
    : new[] { 
        "https://*.railway.app", 
        "https://*.onrender.com",
        "https://your-app-name.railway.app" // Replace with your Railway URL
      };

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

// Ensure uploads directory exists
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads", "movies");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
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

// Serve React static files in production
if (app.Environment.IsProduction())
{
    app.UseDefaultFiles(); // Serves index.html for /
    app.UseStaticFiles(); // Serves static files from wwwroot
    
    // Serve React build files
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "wwwroot")),
        RequestPath = ""
    });
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// CORS must be before UseAuthorization and MapControllers
app.UseCors("AllowReactApp");

// Serve uploads static files
try 
{
    if (Directory.Exists(uploadsPath))
    {
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(builder.Environment.ContentRootPath, "uploads")),
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

// For React Router - serve index.html for unknown API routes
if (app.Environment.IsProduction())
{
    app.MapFallbackToFile("index.html");
}

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MovieDbContext>();
        
        // Wait for database to be ready (Railway might need a moment)
        await Task.Delay(2000);
        
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
            await SeedData.Initialize(services);
            Console.WriteLine("Database seeding completed.");
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
        // Don't crash the app if DB setup fails
    }
}

app.Run();