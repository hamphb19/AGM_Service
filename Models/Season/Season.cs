using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Season
{
    [Table("Season")]
    public class Season : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ShortName { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Required]
        public Farm.Farm Farm { get; set; }

        public ICollection<SeasonField> SeasonFields { get; set; } = new HashSet<SeasonField>();
        public ICollection<Task.Task> Tasks { get; set; } = new List<Task.Task>();

    }
}
