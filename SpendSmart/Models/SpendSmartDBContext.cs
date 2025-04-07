using Microsoft.EntityFrameworkCore;

namespace SpendSmart.Models
{
    public class SpendSmartDBContext:DbContext

    {
        public DbSet<Expense> Expenses { get; set; }
        public SpendSmartDBContext(DbContextOptions<SpendSmartDBContext> options)
            :base(options) 
        {
            
        }
    }
}
