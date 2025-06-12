using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Models;
using Microsoft.EntityFrameworkCore;

namespace SpendSmart.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalApiController : ControllerBase
    {
        private readonly SpendSmartDBContext _context;

        public ExternalApiController(SpendSmartDBContext context)
        {
            _context = context;
        }
        [HttpGet("expenses/{userId}")]
        public async Task<IActionResult> GetExpenses(string userId, [FromHeader(Name = "X-API-KEY")] string apiKey)
        {
            const string MyApiKey = "MySuperSecretKey123";

            if (apiKey != MyApiKey)
                return Unauthorized("Invalid API Key");

            // Query real expense data for the user
            var expenses = await _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => new
                {
                    e.Id,
                    e.UserId,
                    e.Description,
                    e.Value,
                    e.CreatedDate,
                    e.UpdatedDate
                })
                .ToListAsync();

            return Ok(expenses);
        }
    }
}

