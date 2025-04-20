using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using tomb.DB;
using tomb.DTO;
using tomb.Model;
using tomb.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace tomb.Controllers
{


    [ApiController, Route("user"), Authorize, EnableRateLimiting(policyName: "UserControllerRateLimit")] //TODO: Add rate limitter
    public class UserController(ApplicationDBContext db, ILogger<TombController> logger, UserManager<User> userManager, ICacheService cacheService) : ControllerBase
    {
        private readonly ILogger<TombController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ApplicationDBContext _db = db ?? throw new ArgumentNullException(nameof(db));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));


        [HttpGet("{username}")]
        public async Task<IActionResult> GetUser([FromRoute] string username)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            User? userRequested = await _userManager.FindByIdAsync(userId);

            if (userRequested == null)
                return NotFound(new { message = "User not found." });

            User? userFetched = await _userManager.FindByNameAsync(username);

            if (userFetched == null)
                return NotFound(new { message = "User not found!" });

            List<string> roles = (await _userManager.GetRolesAsync(userRequested)).ToList();

            UserFetchDTO data = userFetched.ToUserFetchDTO(roles);

            return Ok(new { data });
        }

        [HttpGet("query/{usernameQuery}")]
        public async Task<IActionResult> GetUsers([FromRoute] string usernameQuery)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested == null)
                return NotFound(new { message = "User not found." });

            string requestPath = Request.Path.ToString();

            List<User>? cache = await _cacheService.GetCachedDataAsync<List<User>>(requestPath);

            if (cache is not null)
            {
                _logger.LogInformation($"Cache data with URL {requestPath} is found. Sending.");
                return Ok(cache);
            }


            List<User> users = await _db.Users
                .Where(u => u.UserName.StartsWith(usernameQuery))
                .ToListAsync();

            List<UserQueryDTO> usersFetched = [];

            foreach (User user in users)
            {
                IList<string> roles = await _userManager.GetRolesAsync(user);
                usersFetched.Add(user.ToUserQueryDTO([.. roles]));
            }

            await _cacheService.SetCacheDataAsync(requestPath, usersFetched, TimeSpan.FromMinutes(10));
            _logger.LogInformation($"Cache data for URL {requestPath} is renewing");

            return Ok(new { data = usersFetched });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { message = "You are not logged in!" });

            User? userRequested = await _userManager.FindByIdAsync(userId);

            if (userRequested == null)
                return NotFound(new { message = "User not found!" });

            List<string> roles = (await _userManager.GetRolesAsync(userRequested)).ToList();

            UserOwnDTO data = userRequested.ToUserOwnDTO(roles);

            return Ok(new { data });
        }


    }
}
