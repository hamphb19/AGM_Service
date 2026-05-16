using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Audit
{
    public abstract class Auditable
    {
        public DateTime CreateDate { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime ChangeDate { get; set; }
        public User? ChangeBy { get; set; }

    }
}
