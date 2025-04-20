using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace tomb.Model
{
    [Table("user")]
    [Index(nameof(UserName), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    public class User : IdentityUser<Guid>
    {
        [Required, MaxLength(50)]
        public override required string UserName { get; set; }

        [Required, EmailAddress, MaxLength(255)]
        public override required string Email { get; set; }

        [Required, MaxLength(30)]
        public string Name { get; set; } = null!;

        [MaxLength(30)]
        public string? Surname { get; set; }

        [MaxLength(1_000_000)]
        public byte[]? Image { get; set; } = null!;

        public DateTime? EmailVerified { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime LastActive { get; set; }

        public virtual List<Session>? Sessions { get; set; }

        public virtual List<Tomb> Tombs { get; set; } = [];
    }
}
