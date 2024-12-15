using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Northwind.Data;
using Northwind.Infrastructure.Filters;
using Northwind.Middleware;
using Northwind.Models;
using Northwind.Services;
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

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies"; // Local login
    options.DefaultChallengeScheme = "AzureAD";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Account/ExternalLogin";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddOpenIdConnect("AzureAD", options =>
{
    options.ClientId = "ClientId";
    options.ClientSecret = "ClientSecret";
    options.Authority = "https://login.microsoftonline.com/b41b72d0-4e9f-4c26-8a69-f949f367c91d/v2.0";
    options.CallbackPath = new PathString("/signin-oidc");
    options.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
    options.ResponseType = "code id_token";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false
    };
});

// Configure Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<NorthwindContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdministratorsOnly", policy =>
        policy.RequireRole("Administrators"));
});

// Configure email sender for password reset (Dummy configuration for testing)
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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

//builder.Services.AddRazorPages(options =>
//{
//    options.Conventions.AuthorizeAreaFolder("Identity", "/Account"); // Authorize access to Account pages
//});

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

// Ensure the "Administrators" role exists and assign it to a user
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    await CreateAdminRoleAsync(serviceProvider);
}

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
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    // Default route configuration
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});
app.MapRazorPages();
app.Run();


async Task CreateAdminRoleAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

    const string adminRole = "Administrators";
    const string adminEmail = "admin@example.com";
    const string adminPassword = "Admin123!";

    // Ensure the "Administrators" role exists
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    // Create and assign the admin user
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        await userManager.CreateAsync(adminUser, adminPassword);
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRole))
    {
        await userManager.AddToRoleAsync(adminUser, adminRole);
    }
}