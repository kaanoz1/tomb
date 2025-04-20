using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using tomb.DB;
using tomb.DTO;
using tomb.Model;
using tomb.Services;

namespace tomb.Controllers
{

    [ApiController, Route("query"), Authorize, EnableRateLimiting(policyName: "QueryControllerRateLimit")] //TODO: Add rate limitter
    public class QueryController(ApplicationDBContext db, ILogger<QueryController> logger, ICacheService cacheService) : ControllerBase
    {
        private readonly ILogger<QueryController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ApplicationDBContext _db = db ?? throw new ArgumentNullException(nameof(db));
        private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));


        [HttpGet("search")]
        public async Task<IActionResult> GetQueryResult([FromQuery] string query)
        {

            if (query.Length > 126) return NotFound();

            const byte takeCount = 20;

            List<TombOwnerDTO> data;

            string requestPath = Request.Path.ToString() + Request.QueryString.ToString();

            List<TombOwnerDTO>? cache = await _cacheService.GetCachedDataAsync<List<TombOwnerDTO>>(requestPath);
            if (cache != null)
            {
                _logger.LogInformation($"Cache data with URL {requestPath} is found. Sending.");
                return Ok(new { data = cache });
            }

            string[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string? containsQuery = string.Join(" AND ", words.Select(w => $"\"{w}\""));

            data = await _db.Tombs.FromSqlRaw("SELECT * FROM tomb WHERE CONTAINS((name, description), {0})", containsQuery)
                                  .AsNoTracking()
                                  .Take(takeCount)
                                  .Include(t => t.User)
                                  .Select(tt => tt.ToTombOwnerTO())
                                  .ToListAsync();


            await _cacheService.SetCacheDataAsync(requestPath, data, TimeSpan.FromMinutes(3));
            _logger.LogInformation($"Cache data for URL {requestPath} is renewing");

            return Ok(new { data });
        }

    }
}
