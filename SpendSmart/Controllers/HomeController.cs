using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using SpendSmart.ViewModels;

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
    public IActionResult Expense()
    {
        var allExpenses= _context.Expenses.ToList();
        var totalExpense = allExpenses.Sum(x => x.Value);
        //ViewBag.Expense = totalExpense;
        var viewModel = new ExpenseViewModel
        {
            TotalExpense = totalExpense,
            Expenses = allExpenses

        };
        //return View(allExpenses);
        return View(viewModel);
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
    public IActionResult CreateEditExpenseForm(Expense expense)
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
