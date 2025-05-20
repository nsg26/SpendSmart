using SpendSmart.Models;

namespace SpendSmart.ViewModels
{
    public class ExpenseViewModel
    {
        public List<Expense> Expenses { get; set; }
        public decimal TotalExpense { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}
