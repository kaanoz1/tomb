using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tomb.Model
{
    [Table("session")]
    public class Session
    {
        [Key]
        public string Key { get; set; } = null!;
        
        public required Guid UserId { get; set; }

        public required DateTime ExpiresAt { get; set; }
    }
}
