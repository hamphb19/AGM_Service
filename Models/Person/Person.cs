using AGM_API.Models.Audit;
using AGM_API.Models.Farm.FarmMember;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AGM_API.Models.Person
{
    [Table("Person")]
    public class Person : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        public string? FirstName { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public User User { get; set; } = null!;  
        public long UserId { get; set; }

        public ICollection<FarmMember> MemberOfFarms { get; set; } = new HashSet<FarmMember>();
        public ICollection<Farm.Farm> OwnerOfFarms { get; set; } = new HashSet<Farm.Farm>();
    }
}
