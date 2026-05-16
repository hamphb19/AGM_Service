using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models
{
    [Table("User")]
    public class User
    {
        [Key, Required]
        public long Id { get; set; }
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        [Required]
        public string Username { get; set; } = null!;
        public Person.Person Person { get; set; }
    }
}
