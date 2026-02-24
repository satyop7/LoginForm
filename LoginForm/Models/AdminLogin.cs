using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginForm.Models
{
    [Table("AdminLogin")]
    public class AdminLogin
    {
        [Key]
        public int AdminId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }

    }
}
