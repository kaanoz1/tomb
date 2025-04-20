using Microsoft.Extensions.FileSystemGlobbing;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using static tomb.Utility.Utility;

namespace tomb.Model
{
    [Table("cache")]
    public class Cache
    {
        [Key, Column("id", TypeName = DBType64bitInteger)]
        public long Id { get; set; }

        [Required, MaxLength(126)]
        public required string Key { get; set; }

        [Required, Column("data", TypeName = DBTypeVARCHARMAX)]
        public required string Data { get; set; }

        [Required, Column(TypeName = DBTypeDateTime)]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow;

    }
}
