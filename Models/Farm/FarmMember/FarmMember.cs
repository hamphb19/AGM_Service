using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Farm.FarmMember
{
    [Table("FarmMember")]
    public class FarmMember
    {
        public long farm_Id { get; set; }
        public long person_Id { get; set; }
        public long role_Id { get; set; }

        public Farm Farm { get; set; } = null!;
        public Person.Person Person { get; set; } = null!;
        public MemberRole Role { get; set; } = null!;
    }
}
