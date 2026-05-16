using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Farm.FarmMember
{
    [Table("MemberRole")]
    public class MemberRole
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name {  get; set; } = null!;
        [Required]
        public string ShortName { get; set; } = null!;

        public ICollection<FarmMember> memberRoles { get; set; } = new HashSet<FarmMember>();
    }
}
