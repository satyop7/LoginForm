using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginForm.Models
{
    [Table("ExamQuestion")]
    public class ExamQuestion
    {
        [Key]
        public int LinkId { get; set; }
        // --- Relationship 1: The Exam ---

        // The Foreign Key column (Raw Data)
        public int ExamId { get; set; }

        // The Navigation Property (The actual object)
        // [ForeignKey("ExamId")] tells EF that 'ExamId' is the key for this object
        [ForeignKey("ExamId")]
        public virtual Exam Exam { get; set; }


        // --- Relationship 2: The Question ---

        // The Foreign Key column
        public int QuestionId { get; set; }

        // The Navigation Property
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}
