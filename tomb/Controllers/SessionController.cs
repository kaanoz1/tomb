using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using tomb.DB;
using tomb.Model;
using tomb.Validation;

namespace tomb.Controllers
{
    [ApiController, Route("session"), Authorize] //TODO: Add rate limitter
    public class SessionController(ApplicationDBContext db, ILogger<SessionController> logger, SignInManager<User> signInManager, UserManager<User> userManager) : ControllerBase
    {
        private readonly ILogger<SessionController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly SignInManager<User> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly ApplicationDBContext _db = db ?? throw new ArgumentNullException(nameof(db));

        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { message = "You are not logged in!" });

            User? UserRequested = await _userManager.FindByIdAsync(userId);

            if (UserRequested == null)
                return NotFound(new { message = "Something went wrong!" });

            await _signInManager.SignOutAsync();

            _logger.LogInformation($"User: [Id: {UserRequested.Id}, Username: {UserRequested.UserName}] successfully logged out.");

            return Ok(new { message = "Successfully logged out" });
        }

        [HttpPut, Route("update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileModel model)
        {
            Console.WriteLine(model.Name);
            Console.WriteLine(model.Surname);
            Console.WriteLine(model.Username);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { message = "You are not logged in!" });

            User? UserRequested = await _userManager.FindByIdAsync(userId);

            if (UserRequested == null)
                return NotFound(new { message = "UserRequested not found!" });

            string updateLogRow = "";

  

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                var existingUser = await _userManager.FindByNameAsync(model.Username);

                if (existingUser != null)
                    return BadRequest(new { message = "Username is already taken!" });
            }


            if (model.Image != null && model.Image.Length > 0)
            {
                using var memoryStream = new MemoryStream();

                await model.Image.CopyToAsync(memoryStream);

                byte[] UpdatedImage = memoryStream.ToArray();

                UserRequested.Image = UpdatedImage;
                updateLogRow += $" Profile Image updated.";
            }


            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                string UpdatedName = model.Name.Trim();

                updateLogRow += $" Name: {UserRequested.Name} -> {UpdatedName}";

                UserRequested.Name = UpdatedName;
            }

            if (!string.IsNullOrWhiteSpace(model.Surname))
            {
                string UpdatedSurname = model.Surname.Trim();

                updateLogRow += $" Surname: {UserRequested.Surname} -> {UpdatedSurname}";
                UserRequested.Surname = UpdatedSurname;
            }

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                string UpdatedUsername = model.Username.Trim();

                updateLogRow += $" UserName: {UserRequested.UserName} -> {UpdatedUsername}";
                UserRequested.UserName = UpdatedUsername;
            }

            IdentityResult updateResult = await _userManager.UpdateAsync(UserRequested);

            if (!updateResult.Succeeded)
            {
                _logger.LogError($"Error occurred, while: User: [Id: {UserRequested.Id}, Username: {UserRequested.UserName}] is update his profile: {updateLogRow}. Error Details: {updateResult.Errors}");
                return BadRequest(new
                {
                    message = "Failed to update profile!",
                    errors = updateResult.Errors.Select(e => e.Description)
                });
            }

            await _db.SaveChangesAsync();


            _logger.LogInformation($"Operation completed. User: [Id: {UserRequested.Id}, Username: {UserRequested.UserName}] has updated his account: {updateLogRow}");
            
            return Ok(new
            {
                message = "Profile updated successfully!",
            });
        }

    }
}