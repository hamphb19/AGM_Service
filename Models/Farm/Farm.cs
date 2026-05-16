using AGM_API.Models.Audit;
using AGM_API.Models.Person;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Farm
{
    [Table("Farm")]
    public class Farm : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string ShortName { get; set; } = null!;
        public Person.Person? Owner { get; set; }
        public ICollection<FarmMember.FarmMember> farmMembers { get; set; } = new HashSet<FarmMember.FarmMember>();
        public ICollection<FarmAnimal> farmAnimals { get; set; } = new List<FarmAnimal>();
    }
}
