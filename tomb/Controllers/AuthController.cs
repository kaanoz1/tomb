using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tomb.DB;
using tomb.Model;
using tomb.Validation;

namespace tomb.Controllers
{
    [ApiController, Route("[controller]")]
    public class AuthController(ApplicationDBContext db, UserManager<User> userManager, RoleManager<Role> roleManager, SignInManager<User> signInManager, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        private readonly SignInManager<User> _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        private readonly ApplicationDBContext _db = db ?? throw new ArgumentNullException(nameof(db));
        private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly RoleManager<Role> _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));


        [HttpPost, Route("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            _logger.LogInformation($"An registration process began.");

            User userCreated = new()
            {
                UserName = model.Username,
                Email = model.Email,
                Name = model.Name,
                Surname = model.Surname ?? null!,
                CreatedAt = DateTime.UtcNow,
                LastActive = DateTime.UtcNow,

            };

            if (model.Image != null)
            {
                using var memoryStream = new MemoryStream();
                await model.Image.CopyToAsync(memoryStream);
                userCreated.Image = memoryStream.ToArray();
            }

            var Result = await _userManager.CreateAsync(userCreated, model.Password);

            if (Result.Succeeded)
            {
                _logger.LogInformation($"Operation completed: New User information: Identifier: {userCreated.Id}, Username: {userCreated.UserName}, Email: {userCreated.Email}, Name: {userCreated.Name} Surname: {userCreated.Surname} CreatedAt: {userCreated.CreatedAt}");

                await _db.SaveChangesAsync();


                await _signInManager.SignInAsync(userCreated, isPersistent: true);

                return Ok(new
                {
                    message = "Registration successful!",
                });
            }

            if (Result.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                _logger.LogInformation($"Registration failed: Duplicated username tried to be created. Duplicated Username: {userCreated.UserName} Email: {userCreated.Email}");
                return Conflict(new { message = "Username already exists." });
            }

            if (Result.Errors.Any(e => e.Code == "DuplicateEmail"))
            {
                _logger.LogInformation($"Registration failed: Duplicated Email tried to be created. Duplicated Email: {userCreated.Email} Username: {userCreated.UserName}");
                return Conflict(new { message = "Email already exists." });
            }

            foreach (IdentityError error in Result.Errors)
                _logger.LogWarning($"Registration failed: {error.Code} - {error.Description}");

            return BadRequest(new { message = string.Join(", ", Result.Errors.Select(e => e.Description).ToList()) });
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {

            User? userRequested = await _userManager.FindByEmailAsync(model.Email);

            if (userRequested == null)
            {
                _logger.LogInformation($"Login failed: User with that name is not found. Claimed Email: {model.Email}");
                return Unauthorized(new { message = "Invalid Credentials!" });
            }

            var Result = await _signInManager.CheckPasswordSignInAsync(userRequested, model.Password, lockoutOnFailure: false);

            if (!Result.Succeeded)
            {
                _logger.LogInformation($"Password is incompatible. Claimed password: {model.Password} for User: [Id: {userRequested.Id}]");
                return Unauthorized(new { message = "Invalid Credentials!" });
            }

            //if (userRequested.IsFrozen != null)
            //{

            //    FreezeR freezeR = new()
            //    {
            //        UserId = userRequested.Id,
            //        Status = FreezeStatus.Unfrozen
            //    };

            //    userRequested.IsFrozen = null;

            //    _db.FreezeR.Add(freezeR);

            //    await _db.SaveChangesAsync();
            //    await _userManager.UpdateAsync(userRequested);
            //    _logger.LogInformation($"User had been frozen, melted, User: [Id: {userRequested.Id}, Username: {userRequested.UserName}]");
            //}

            await _signInManager.SignInAsync(userRequested, isPersistent: true);

            _logger.LogInformation($"User: [Id: {userRequested.Id}, Username: {userRequested.UserName}] has logged in.");

            return Ok(new { message = "Successfully logged in!" });
        }
    }
}
    