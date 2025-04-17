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
                if (entry.Value.Errors.Count > 0)
                {
                    var errorMessages = entry.Value.Errors.Select(e => e.ErrorMessage).ToList();
                    Console.WriteLine($"Field: {entry.Key}, Errors: {string.Join(", ", errorMessages)}");
                }
            }
            return View(expense);
        }
        if (ModelState.IsValid)
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
