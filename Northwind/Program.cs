using Microsoft.EntityFrameworkCore;
using Northwind.Data;
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

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
