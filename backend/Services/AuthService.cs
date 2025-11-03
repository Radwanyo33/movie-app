using Microsoft.EntityFrameworkCore;
using Live_Movies.Models;
using Live_Movies.Data;

namespace Live_Movies.Services
{
    public interface IAuthService 
    {
        Task<bool> ValidateUserAsync(LoginModel login);
        Task<bool> CreateUserAsync(string email, string password);
    
    }
    public class AuthService : IAuthService
    {
        private readonly MovieDbContext _context;

        public AuthService(MovieDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateUserAsync(LoginModel login)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == login.Email && u.IsActive);
            if (user != null)
            {
                var isValid = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
                if (isValid) {
                    user.LastLogin = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }

            }
            return false;           
        }

        public async Task<bool> CreateUserAsync(string email, string password)
        {
            if(await _context.Users.AnyAsync(u=> u.Email == email))
            {
                return false;
            }

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
