using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SpendSmart.Models
{
    public class SpendSmartDBContext: IdentityDbContext

    {
        public DbSet<Expense> Expenses { get; set; }
        public SpendSmartDBContext(DbContextOptions<SpendSmartDBContext> options)
            :base(options) 
        {
            
        }
    }
}
