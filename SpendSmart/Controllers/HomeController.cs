using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SpendSmart.Models;
using SpendSmart.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpendSmart.Controllers;
[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SpendSmartDBContext _context;
    public HomeController(ILogger<HomeController> logger, SpendSmartDBContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        var username = User.Identity.Name;
        var viewModel = new WelcomeViewModel
        {
            Username = username
        };
        return View(viewModel);
    }
    public async Task<IActionResult> Expense(int page = 1)
    {
        const int pageSize = 10;
        try
        {
            //  Get current user's ID
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            //var allExpenses = await _context.Expenses.ToListAsync();
            //  Only fetch data for that user
            // var allExpenses = await _context.Expenses
            // .Where(x => x.UserId == currentUserId)
            // .ToListAsync();

            // Query all expenses for the current user
            var allExpenses = _context.Expenses
            .Where(x => x.UserId == currentUserId)
            .OrderByDescending(x => x.Id); // Optional: newest first

            //var totalExpense = allExpenses.Sum(x => x.Value);

            // Total count of all user's expenses
            var totalCount = await allExpenses.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Paginated expenses (only current page)
            var expenses = await allExpenses
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            // Total of all expenses across all pages for this user

            var totalExpense = await _context.Expenses
            .Where(x => x.UserId == currentUserId)
            .SumAsync(x => x.Value);

            // _logger.LogInformation("Successfully loaded {Count} expenses.", allExpenses.Count);
            // throw new InvalidOperationException("Simulated database error!");
            //ViewBag.Expense = totalExpense;

            // Prepare the view model
            var viewModel = new ExpenseViewModel
            {
                Expenses = expenses,
                TotalExpense = totalExpense,
                CurrentPage = page,
                PageSize = pageSize,          //  Needed for serial number logic
                TotalPages = totalPages
            };
            _logger.LogInformation("Loaded page {Page} with {Count} expenses.", page, expenses.Count);
            return View(viewModel);
        }
        catch(Exception Ex)
        {
            _logger.LogError(Ex, "Failed to load expenses.");
            return View("Error");
        }
    }
    public IActionResult CreateEditExpense(int? id)
    {
        if(id!=null)
        {
            var ExpenseInDb = _context.Expenses.SingleOrDefault(e => e.Id == id);
            return View(ExpenseInDb);
        }
        
        return View();

    }
    public IActionResult DeleteExpense(int id)
    {
        var ExpenseInDb = _context.Expenses.SingleOrDefault(e => e.Id == id);
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        // Protect from deleting others' expenses
        if (ExpenseInDb == null || ExpenseInDb.UserId != currentUserId)
        {
            return Unauthorized();
        }
        _context.Expenses.Remove(ExpenseInDb);
        _context.SaveChanges();
        return RedirectToAction("Expense");

    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateEditExpenseForm(Expense expense)
    {
        if (!ModelState.IsValid)
        {
            foreach (var entry in ModelState)
            {
                var key = entry.Key;
                var errors = entry.Value.Errors;

                foreach (var error in errors)
                {
                    // Replace this with your actual logging logic
                    _logger.LogWarning($"Validation error on '{key}': {error.ErrorMessage}");
                }
            }
            return View(expense);
        }
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (ModelState.IsValid)
        {
            try
            {
                if (expense.Id == null||expense.Id == 0)
                {
                    //_context.Expenses.Add(expense);
                    //  Attach logged-in user ID during insert
                    expense.UserId = currentUserId;
                    expense.CreatedDate = DateTime.Now;
                    _context.Expenses.Add(expense);

                }
                else
                {
                    var existing = _context.Expenses.AsNoTracking().FirstOrDefault(x => x.Id == expense.Id);
                    if (existing == null || existing.UserId != currentUserId)
                    {
                        return Unauthorized();
                    }
                    expense.UserId = currentUserId;
                    expense.UpdatedDate = DateTime.Now;
                    _context.Expenses.Update(expense);
                }

                _context.SaveChanges();
                return RedirectToAction("Expense");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving expense.");
            }
        }
      
        return View("CreateEditExpense", expense);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
