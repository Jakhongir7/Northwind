using Microsoft.EntityFrameworkCore;
using Northwind.Data;
using Northwind.Infrastructure.Filters;
using Northwind.Middleware;
using Northwind.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/Northwind.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("ApplicationLocation", AppContext.BaseDirectory) // Include application location
    .CreateLogger();

builder.Host.UseSerilog(); // Use Serilog for logging

builder.Services.AddDbContext<NorthwindContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NorthwindDb")));

// Register configuration section for product settings
builder.Services.Configure<ProductSettings>(builder.Configuration.GetSection("ProductSettings"));

// Register the logging filter
builder.Services.AddScoped<LoggingActionFilter>(provider =>
    new LoggingActionFilter(provider.GetRequiredService<ILogger<LoggingActionFilter>>(), logParameters: false));

// Add MVC services
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<LoggingActionFilter>(); // Add the filter globally
});

// Create configuration options
var imageCacheOptions = new ImageCachingOptions
{
    CacheDirectory = Path.Combine(builder.Environment.WebRootPath, "ImageCache"),
    MaxCachedImages = 50, // Adjust max cached images count as needed
    CacheExpiration = TimeSpan.FromMinutes(10) // Adjust expiration time as needed
};

// Register and use the middleware
builder.Services.AddSingleton(imageCacheOptions);
builder.Services.AddSingleton<ILogger<ImageCachingMiddleware>, Logger<ImageCachingMiddleware>>();

var app = builder.Build();

// Log application startup
Log.Information("Application started. Location: {Location}", AppContext.BaseDirectory);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();  // Show detailed error page in development
}

// Add the custom middleware
app.UseMiddleware<ImageCachingMiddleware>(imageCacheOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    // Default route configuration
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


app.Run();
