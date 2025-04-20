using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace tomb.Model
{
    [Table("tomb"), Index(nameof(Latitude), nameof(Longitude))]
    public class Tomb
    {
        [Required]
        public long Id { get; set; }

        [Required, MaxLength(50), MinLength(1)]
        public required string Name { get; set; }

        [MaxLength(120)]
        public string? Description { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public required DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public required Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

    }

}
