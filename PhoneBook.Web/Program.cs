using Microsoft.EntityFrameworkCore;
using PhoneBook.Infrastructure.Data;
using PhoneBook.Infrastructure.Repositories;
using PhoneBook.Domain.Interfaces;
using PhoneBook.Application.Services;
using FluentValidation;
using PhoneBook.Application.Validators;
using PhoneBook.Application.DTOs;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
// ============================================================
// 1. Configure Services
// ============================================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// ============================================================
// 2. Database Configuration
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsAssembly("PhoneBook.Infrastructure");
        sqlOptions.CommandTimeout(60);
        // sqlOptions.EnableRetryOnFailure(
        //     maxRetryCount: 5,
        //     maxRetryDelay: TimeSpan.FromSeconds(30),
        //     errorNumbersToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ============================================================
// 3. Register Repositories & Unit of Work
// ============================================================
builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactImageRepository, ContactImageRepository>();
builder.Services.AddScoped<IUnitOfWork, PhoneBook.Infrastructure.UnitOfWork.UnitOfWork>();

// ============================================================
// 4. Register Application Services
// ============================================================
builder.Services.AddScoped<IContactService, ContactService>();


// ============================================================
// 5. Register Validators
// ============================================================
builder.Services.AddScoped<IValidator<CreateContactDto>, CreateContactValidator>();
builder.Services.AddScoped<IValidator<UpdateContactDto>, UpdateContactValidator>();



// ============================================================
// 7. Logging Configuration
// ============================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
}

// ============================================================
// 8. Session Configuration
// ============================================================
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================================
// 9. Response Compression
// ============================================================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// ============================================================
// Build Application
// ============================================================
var app = builder.Build();

// ============================================================
// Configure Middleware Pipeline
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCompression();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ============================================================
// Configure Endpoints
// ============================================================


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Contacts}/{action=Index}/{id?}");

// ============================================================
// Database Auto-Migration (Development Only)
// ============================================================
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            if (context.Database.GetPendingMigrations().Any())
            {
                app.Logger.LogInformation("Applying pending migrations...");
                context.Database.Migrate();
                app.Logger.LogInformation("Migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

// ============================================================
// Configure Culture (Persian Support)
// ============================================================
// var supportedCultures = new[] { new CultureInfo("fa-IR") };
// app.UseRequestLocalization(new RequestLocalizationOptions
// {
//     DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("fa-IR"),
//     SupportedCultures = supportedCultures,
//     SupportedUICultures = supportedCultures
// });
// // ==================== تنظیم Culture به میلادی ====================
var supportedCultures = new[] { new CultureInfo("en-US") };
var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-US"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

// تنظیم پیش‌فرض Thread
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

// اجبار استفاده از تقویم میلادی
// var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.Calendar = new GregorianCalendar();
Thread.CurrentThread.CurrentCulture = cultureInfo;
Thread.CurrentThread.CurrentUICulture = cultureInfo;

// ============================================================
// Run Application
// ============================================================
app.Logger.LogInformation("🚀 PhoneBook Application started successfully!");

app.Run();

public partial class Program { }