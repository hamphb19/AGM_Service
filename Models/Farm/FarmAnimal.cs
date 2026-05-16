using AGM_API.Models.Audit;
using AGM_API.Models.Farm.FarmMember;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Farm
{
    [Table("FarmAnimal")]
    public class FarmAnimal : Auditable
    {
        public long farm_Id { get; set; }
        public long animal_Id { get; set; }

        public Farm Farm { get; set; } = null!;
        public Animal.Animal Animal { get; set; } = null!;
        public int AnimalCount { get; set; } = 0;

    }
}
