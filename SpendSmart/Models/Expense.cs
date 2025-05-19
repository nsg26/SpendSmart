using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace SpendSmart.Models
{
    public class Expense
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Value is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Value must be positive.")]
        public decimal Value { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Description must be 3-100 characters.")]
        public string? Description { get; set; }
        // Add this line to store the logged-in user ID
        public string? UserId { get; set; }
    }
}
