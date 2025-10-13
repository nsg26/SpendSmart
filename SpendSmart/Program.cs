using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpendSmart.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SpendSmart.Services;

namespace SpendSmart
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddControllersWithViews();

            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            builder.Services.AddDbContext<SpendSmartDBContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SpendSmartDBContext>();

            builder.Logging.AddConsole().AddDebug();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                options.SlidingExpiration = true;
            });

            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.WebHost.ConfigureKestrel(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.ListenLocalhost(5034);
                    options.ListenLocalhost(7199, listenOptions =>
                    {
                        listenOptions.UseHttps();
                    });
                }
                else
                {
                    var kestrelSection = builder.Configuration.GetSection("Kestrel");
                    if (kestrelSection.Exists())
                    {
                        options.Configure(kestrelSection);
                    }
                }
            });

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<CurrencyService>();

            var app = builder.Build();

            // ===========================================
            // ✅ Azure-safe Migration Logic (Auto apply)
            // ===========================================
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SpendSmartDBContext>();
                try
                {
                    Console.WriteLine("Applying EF Core migrations...");
                    dbContext.Database.Migrate();
                    Console.WriteLine("EF Core migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ EF Core migration failed: {ex.Message}");
                }

                // Create Roles and Admin User if not exist
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string[] roles = { "Admin", "User" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                string adminEmail = "admin@spendsmart.com";
                string adminPassword = "Admin@123";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    var user = new IdentityUser { UserName = adminEmail, Email = adminEmail };
                    var result = await userManager.CreateAsync(user, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Urls.Add("http://0.0.0.0:8080");
            app.Run();
        }
    }
}
