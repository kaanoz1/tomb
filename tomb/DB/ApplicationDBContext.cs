using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using tomb.Model;

namespace tomb.DB
{
    public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : IdentityDbContext<User, Role, Guid>(options)
    {
        public DbSet<Session> Sessions { get; set; }

        public DbSet<Tomb> Tombs { get; set; }
        public DbSet<Cache> Caches { get; set; }
    }
}
