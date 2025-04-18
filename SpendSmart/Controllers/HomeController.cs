using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SpendSmart.Models;
using SpendSmart.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SpendSmart.Controllers;

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
        return View();
    }
    public async Task<IActionResult> Expense()
    {
        try
        {
            var allExpenses = await _context.Expenses.ToListAsync();
            var totalExpense = allExpenses.Sum(x => x.Value);
            _logger.LogInformation("Successfully loaded {Count} expenses.", allExpenses.Count);
            // throw new InvalidOperationException("Simulated database error!");
            //ViewBag.Expense = totalExpense;
            var viewModel = new ExpenseViewModel
            {
                TotalExpense = totalExpense,
                Expenses = allExpenses

            };
            //return View(allExpenses);
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

        if (ModelState.IsValid)
        {
            try
            {
                if (expense.Id == 0)
                {
                    _context.Expenses.Add(expense);

                }
                else
                {
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
