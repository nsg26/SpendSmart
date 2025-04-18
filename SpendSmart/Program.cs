using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpendSmart.Models;
using SpendSmart.Services;

namespace SpendSmart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<SpendSmartDBContext>(options => options.UseInMemoryDatabase("SpendSmartDB"));
            builder.Logging.AddConsole().AddDebug();
            builder.Services.AddSingleton<UserService>();
            // Add cookie authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Auth/Login"; // Path to login page
                options.AccessDeniedPath = "/Auth/AccessDenied"; // Path for access denied
                options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Cookie expiration
                options.SlidingExpiration = true; // Reset expiration timer on activity
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // Enable authentication
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
