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

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            // Access configuration this way
            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                      ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
            builder.Services.AddDbContext<SpendSmartDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add Identity services
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<SpendSmartDBContext>();
            builder.Logging.AddConsole().AddDebug();
            // builder.Services.AddSingleton<UserService>();
            // Add cookie authentication
            /* builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
             .AddCookie(options =>
             {
                 options.LoginPath = "/Account/Login"; // Path to login page
                 options.AccessDeniedPath = "/Account/AccessDenied"; // Path for access denied
                 options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Cookie expiration
                 options.SlidingExpiration = true; // Reset expiration timer on activity
             }); */

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1);
                options.SlidingExpiration = true;
            });
            // Load additional configuration files
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.WebHost.ConfigureKestrel(options =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    options.ListenLocalhost(5034); // HTTP
                    options.ListenLocalhost(7199, listenOptions => // HTTPS
                    {
                        listenOptions.UseHttps();
                    });
                }
                else
                {
                    // Apply configuration from file but with validation
                    var kestrelSection = builder.Configuration.GetSection("Kestrel");
                    if (kestrelSection.Exists())
                    {
                        options.Configure(kestrelSection);

                        // Additional validation for Docker
                        if (builder.Environment.EnvironmentName == "Docker")
                        {
                            if (!File.Exists("/app/https/aspnetapp.pfx"))
                            {
                                throw new FileNotFoundException("Missing HTTPS certificate in Docker container");
                            }
                        }
                    }
                }
            });
            builder.Services.AddHttpClient(); // For API calls
            builder.Services.AddScoped<CurrencyService>(); // Register your service
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SpendSmartDBContext>();

                // Apply any pending migrations (creates AspNetRoles, AspNetUsers, etc.)
                dbContext.Database.Migrate();

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                string[] roles = { "Admin", "User" };

                // Create roles if not exist
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Create Admin User
                string adminEmail = "admin@spendsmart.com";
                string adminPassword = "Admin@123"; // Strong password recommended
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


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                // Auto-run migrations in production
               /* using (var scope = app.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<SpendSmartDBContext>();
                    dbContext.Database.Migrate();
                } */
            }
           

           // if (app.Environment.IsDevelopment())
           // {
                app.UseSwagger();
                app.UseSwaggerUI();
           // }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.Urls.Add("http://0.0.0.0:8080");
            app.Run();
        }
    }
}
