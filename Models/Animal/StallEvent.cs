using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Animal
{
    public enum StallEventType
    {
        Incoming = 0,
        Outgoing = 1,
        Birth = 2,
        Death = 3,
        Other = 4,
    }

    [Table("StallEvent")]
    public class StallEvent
    {
        [Key, Required]
        public long Id { get; set; }
        public long StallId { get; set; }
        public Stall Stall { get; set; } = null!;
        public DateTime Date { get; set; }
        public StallEventType EventType { get; set; }
        public long? AnimalTypeId { get; set; }
        public AnimalType? AnimalType { get; set; }
        public int Count { get; set; }
        public string? Notes { get; set; }
    }
}
