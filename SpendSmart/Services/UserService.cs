using SpendSmart.Models;

namespace SpendSmart.Services
{
    public class UserService
    {
        private readonly List<User> _users = new List<User>
    {
        new User { Username = "admin", Password = "admin123" } // Hardcoded user
    };

        public bool ValidateUser(string username, string password)
        {
            return _users.Any(u => u.Username == username && u.Password == password);
        }
    }
}
