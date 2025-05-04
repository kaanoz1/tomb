using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using tomb.DB;
using tomb.DTO;
using tomb.Model;
using tomb.Validation;

namespace tomb.Controllers
{
    [ApiController, Route("tomb"), Authorize, EnableRateLimiting(policyName: "TombControllerRateLimit")] //TODO: Add rate limitter
    public class TombController(ApplicationDBContext db, ILogger<TombController> logger, UserManager<User> userManager) : ControllerBase
    {
        private readonly ILogger<TombController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ApplicationDBContext _db = db ?? throw new ArgumentNullException(nameof(db));
        private readonly UserManager<User> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));


        [HttpGet("")]
        public async Task<IActionResult> GetTombs()
        {

            const int tombCount = 100;

            _logger.LogInformation($"Fetching ALL (Max {tombCount}) tombs for current user.");
            
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to fetch tombs.");
                return Unauthorized(new { message = "You are not logged in!" });
            }

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested == null)
            {
                _logger.LogError("User not found during tomb fetch. UserId: {UserId}", userId);
                return NotFound(new { message = "User not found!" });
            }

            try
            {
                List<TombOwnerDTO> data = await _db.Tombs
                   .Where(t => t.UserId == Guid.Parse(userId)).OrderByDescending(t => t.Id)
                   .Take(tombCount)
                   .Select(t => t.ToTombOwnerDTO())
                   .ToListAsync();

                _logger.LogInformation("Returned {Count} tombs for user {UserId}.", data.Count, userId);

                return Ok(new { data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tombs for user {UserId}", userId);
                return BadRequest(new { message = "Something went wrong" });
            }
        }



        [HttpGet("tombs")]
        public async Task<IActionResult> GetMyTombs()
        {
            _logger.LogInformation("Fetching tombs for current user.");

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to fetch tombs.");
                return Unauthorized(new { message = "You are not logged in!" });
            }

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested == null)
            {
                _logger.LogError("User not found during tomb fetch. UserId: {UserId}", userId);
                return NotFound(new { message = "User not found!" });
            }

            try
            {
                List<TombOwnDTO> data = await _db.Tombs
                   .Where(t => t.UserId == Guid.Parse(userId))
                   .Select(t => t.ToTombOwnDTO())
                   .ToListAsync();

                _logger.LogInformation("Returned {Count} tombs for user {UserId}.", data.Count, userId);

                return Ok(new { data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tombs for user {UserId}", userId);
                return BadRequest(new { message = "Something went wrong" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            _logger.LogInformation("Fetching tomb by Id {TombId}.", id);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to fetch tomb.");
                return Unauthorized(new { message = "You are not logged in!" });
            }

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested is null)
            {
                _logger.LogError("User not found during GetById. UserId: {UserId}", userId);
                return NotFound(new { message = "User not found!" });
            }

            Tomb? tomb = await _db.Tombs.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userRequested.Id);
            if (tomb is null)
            {
                _logger.LogWarning("Tomb not found. TombId: {TombId}, UserId: {UserId}", id, userId);
                return NotFound();
            }

            _logger.LogInformation("Tomb retrieved successfully. TombId: {TombId}", id);
            return Ok(tomb);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] TombCreateValidatedModel model)
        {
            _logger.LogInformation("Creating a new tomb.");

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to create tomb.");
                return Unauthorized(new { message = "You are not logged in!" });
            }

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested == null)
            {
                _logger.LogError("User not found during create. UserId: {UserId}", userId);
                return NotFound(new { message = "User not found!" });
            }

            const int maxTombCount = 50;

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                int tombCount = await _db.Tombs
                    .Where(t => t.UserId == userRequested.Id)
                    .CountAsync();

                if (tombCount >= maxTombCount)
                {
                    _logger.LogWarning("User {UserId} exceeded tomb limit ({MaxTombCount}).", userId, maxTombCount);
                    return BadRequest(new { message = $"You exceeded the limit for adding more tombs ({maxTombCount})" });
                }

                Tomb tomb = new()
                {
                    Name = model.Name,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    UserId = userRequested.Id
                };

                _db.Tombs.Add(tomb);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Tomb created successfully. TombId: {TombId}, UserId: {UserId}", tomb.Id, userId);

                return CreatedAtAction(nameof(GetById), new { id = tomb.Id }, new { message = "Created successfully", tomb.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating tomb. UserId: {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while creating the tomb.", error = ex.Message });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] TombUpdateValidatedModel model)
        {
            _logger.LogInformation("Updating tomb {TombId}.", model.Id);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to update tomb.");
                return Unauthorized(new { message = "You are not logged in!" });
            }

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested == null)
            {
                _logger.LogError("User not found during update. UserId: {UserId}", userId);
                return NotFound(new { message = "User not found!" });
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                Tomb? tombToBeUpdated = _db.Tombs.FirstOrDefault(t => t.Id == model.Id && t.UserId == userRequested.Id);
                if (tombToBeUpdated is null)
                {
                    _logger.LogWarning("Tomb not found or unauthorized update attempt. TombId: {TombId}, UserId: {UserId}", model.Id, userId);
                    return NotFound(new { message = "Tomb not found! Might be deleted." });
                }

                bool hasChanges = false;

                if (string.IsNullOrWhiteSpace(model.Name) is false && model.Name != tombToBeUpdated.Name)
                {
                    tombToBeUpdated.Name = model.Name;
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(model.Description) is false && model.Description != tombToBeUpdated.Description)
                {
                    tombToBeUpdated.Description = model.Description;
                    hasChanges = true;
                }

                if (!hasChanges)
                {
                    _logger.LogInformation("No changes detected in tomb update request. TombId: {TombId}", model.Id);
                    return BadRequest(new { message = "No changes were detected." });
                }

                tombToBeUpdated.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Tomb updated successfully. TombId: {TombId}", model.Id);
                return Ok(new { message = "Tomb updated successfully.", tombToBeUpdated.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating tomb. TombId: {TombId}, UserId: {UserId}", model.Id, userId);
                return StatusCode(500, new { message = "An error occurred while updating the tomb.", error = ex.Message });
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] TombIdentifierModel model)
        {
            _logger.LogInformation("Deleting tomb {TombId}.", model.Id);

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to delete tomb.");
                return Unauthorized(new { message = "You are not logged in!" });
            }

            User? userRequested = await _userManager.FindByIdAsync(userId);
            if (userRequested == null)
            {
                _logger.LogError("User not found during delete. UserId: {UserId}", userId);
                return NotFound(new { message = "User not found!" });
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                Tomb? tombToBeDeleted = _db.Tombs.FirstOrDefault(t => t.Id == model.Id && t.UserId == userRequested.Id);
                if (tombToBeDeleted is null)
                {
                    _logger.LogWarning("Tomb not found or already deleted. TombId: {TombId}, UserId: {UserId}", model.Id, userId);
                    return NotFound(new { message = "Tomb not found! Might be deleted already." });
                }

                _db.Tombs.Remove(tombToBeDeleted);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Tomb deleted successfully. TombId: {TombId}, UserId: {UserId}", model.Id, userId);
                return Ok(new { message = "Tomb deleted successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while deleting tomb. TombId: {TombId}, UserId: {UserId}", model.Id, userId);
                return StatusCode(500, new { message = "An error occurred while deleting the tomb.", error = ex.Message });
            }
        }
    }

}
