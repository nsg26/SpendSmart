using SpendSmart.Models;

namespace SpendSmart.ViewModels
{
    public class ExpenseViewModel
    {
        public List<Expense> Expenses { get; set; }
        public decimal TotalExpense { get; set; }
    }
}
