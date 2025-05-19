using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Services;
using System.Security.Claims;
//not in use
namespace SpendSmart.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, isPersistent: false, lockoutOnFailure: false);
            /* if (_userService.ValidateUser(username, password))
             {
                     // Create claims for the user
                     var claims = new List<Claim>
                     {
                         new Claim(ClaimTypes.Name, username)
                     };

                     // Create identity
                     var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                     var principal = new ClaimsPrincipal(identity);

                     // Sign in the user
                     await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                     return RedirectToAction("Index", "Home");
             } */
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
