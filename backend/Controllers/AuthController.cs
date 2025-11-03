using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Live_Movies.Models;
using Live_Movies.Services;

namespace Live_Movies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost ("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid Input" });

                var isValid = await _authService.ValidateUserAsync(login);

                if (isValid)
                {
                    // Set session variables
                    HttpContext.Session.SetString("IsAdmin", "true");
                    HttpContext.Session.SetString("AdminEmail", login.Email);
                    HttpContext.Session.SetString("LoginTime", DateTime.UtcNow.ToString());

                    return Ok(new { success = true, message = "Login Successful" });
                }
                return Unauthorized(new { success = false, message = "Invalid Credentials" });
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Error during Login");
                return StatusCode(500, new { success = false, message = "Internal Server Error" });
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { success = true, message = "Logout Successful" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginModel registerModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Invalid Input" });

                var result = await _authService.CreateUserAsync(registerModel.Email, registerModel.Password);
                if (result)
                {
                    return Ok(new { success = true, message = "User Created Successfully" });
                }
                return BadRequest(new { success = false, message = "User already exists" });
            }catch(Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { success = false, message = "Internal Server Error" });
            }
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            return Ok(new { isAdmin = !string.IsNullOrEmpty(isAdmin) });
        }
    }
}
