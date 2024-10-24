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

var app = builder.Build();
var env = builder.Environment;

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

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configure the ImageCachingMiddleware options
var imageCacheOptions = new ImageCachingOptions
{
    CacheDirectory = Path.Combine(env.WebRootPath, "ImageCache"),
    MaxCachedImages = 50, // Set your max cached images count
    CacheExpiration = TimeSpan.FromMinutes(10) // Set your expiration time
};

// Add the custom middleware
//app.UseMiddleware<ImageCachingMiddleware>(imageCacheOptions);

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
