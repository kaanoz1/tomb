using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using tomb.DB;
using tomb.Model;

namespace tomb.Services
{
    public class SessionStore : ITicketStore
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SessionStore> _logger;
        private readonly TimeSpan _ticketExpiration = TimeSpan.FromDays(3);

        public SessionStore(IServiceProvider serviceProvider, ILogger<SessionStore> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            _logger.LogInformation("StoreAsync called to create a new Sessions.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                var userIdClaim = ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogError("StoreAsync failed: UserId claim not found in the authentication ticket.");
                    throw new InvalidOperationException("UserId claim not found.");
                }

                if (!Guid.TryParse(userIdClaim, out Guid userId))
                {
                    _logger.LogError($"StoreAsync failed: Invalid UserId format '{userIdClaim}'.");
                    throw new InvalidOperationException("Invalid UserId format.");
                }

                var SessionId = Guid.NewGuid().ToString();
                _logger.LogInformation($"Generated new SessionsId: {SessionId} for UserId: {userId}.");

                var Session = new Session
                {
                    Key = SessionId,
                    UserId = userId,
                    ExpiresAt = DateTime.UtcNow.Add(_ticketExpiration)
                };

                try
                {
                    db.Sessions.Add(Session);
                    await db.SaveChangesAsync();
                    _logger.LogInformation($"Sessions with SessionsId: {SessionId} successfully stored in the database.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "StoreAsync encountered an error while saving the Sessions to the database.");
                    throw;
                }

                return SessionId;
            }
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            _logger.LogInformation($"RenewAsync called for SessionsId: {key}.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                try
                {
                    var Sessions = await db.Sessions.FirstOrDefaultAsync(s => s.Key == key);
                    if (Sessions == null)
                    {
                        _logger.LogWarning($"RenewAsync: SessionsId: {key} not found.");
                        return;
                    }

                    if (Sessions.ExpiresAt < DateTime.UtcNow)
                    {
                        _logger.LogWarning($"RenewAsync: SessionsId: {key} has already expired at {Sessions.ExpiresAt}.");
                        return;
                    }

                    Sessions.ExpiresAt = DateTime.UtcNow.Add(_ticketExpiration);
                    db.Sessions.Update(Sessions);
                    await db.SaveChangesAsync();

                    _logger.LogInformation($"RenewAsync: SessionsId: {key} expiration updated to {Sessions.ExpiresAt}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"RenewAsync encountered an error while renewing the Sessions with SessionsId: {key}.");
                    throw;
                }
            }
        }

        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            _logger.LogInformation($"RetrieveAsync called for SessionsId: {key}.");

            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

            try
            {
                var Sessions = await db.Sessions.FirstOrDefaultAsync(s => s.Key == key);

                if (Sessions == null)
                {
                    _logger.LogWarning($"RetrieveAsync: SessionsId: {key} not found.");
                    return null;
                }

                if (Sessions.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning($"RetrieveAsync: SessionsId: {key} has expired at {Sessions.ExpiresAt}.");
                    return null;
                }

                User? user = await db.Users.FindAsync(Sessions.UserId);

                if (user == null)
                {
                    _logger.LogWarning($"RetrieveAsync: UserId: {Sessions.UserId} associated with SessionsId: {key} not found.");
                    return null;
                }

                var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new(ClaimTypes.Name, user.Id.ToString()),
                        new(ClaimTypes.UserData, user.UserName ?? "\\0"),
                    };

                var identity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);
                var principal = new ClaimsPrincipal(identity);

                var props = new AuthenticationProperties
                {
                    ExpiresUtc = Sessions.ExpiresAt,
                    IsPersistent = true,
                    AllowRefresh = true,
                };

                var ticket = new AuthenticationTicket(principal, props, IdentityConstants.ApplicationScheme);
                _logger.LogInformation($"RetrieveAsync: Authentication ticket created for SessionsId: {key}.");

                user.LastActive = DateTime.UtcNow;
                await db.SaveChangesAsync();
                _logger.LogInformation($"User: [Id: {user.Id}, Username: {user.UserName}]'s last active property has been updated.");

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RetrieveAsync encountered an error while retrieving the Sessions with SessionsId: {key}.");
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            _logger.LogInformation($"RemoveAsync called for SessionsId: {key}.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();

                try
                {
                    var Sessions = await db.Sessions.FirstOrDefaultAsync(s => s.Key == key);

                    if (Sessions == null)
                    {
                        _logger.LogWarning($"RemoveAsync: SessionsId: {key} not found.");
                        return;
                    }

                    db.Sessions.Remove(Sessions);
                    await db.SaveChangesAsync();

                    _logger.LogInformation($"RemoveAsync: SessionsId: {key} successfully removed from the database.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"RemoveAsync encountered an error while removing the Sessions with SessionsId: {key}.");
                    throw;
                }
            }
        }
    }
}