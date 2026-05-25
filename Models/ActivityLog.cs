using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models
{
    [Table("ActivityLog")]
    public class ActivityLog
    {
        [Key]
        public long Id { get; set; }
        public long FarmId { get; set; }
        public long? UserId { get; set; }
        public string UserDisplayName { get; set; } = "";
        public string EntityType { get; set; } = "";
        public long? EntityId { get; set; }
        public string Action { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
