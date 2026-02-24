using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace LoginForm.Models
{
    [Table("Result")]
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        // ADDED: We must track which exam this result is for
        public int ExamId { get; set; }

        public string? StudentEmail { get; set; }
        public int TotalQuestion { get; set; }
        public int CorrectlyAnswered { get; set; }
        public bool IsPassed { get; set; }
    }
}
