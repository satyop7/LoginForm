using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginForm.Models
{
    [Table("Exam")]
    public class Exam
    {
        [Key]
        public int ExamId { get; set; }

        // REMOVED: public string StudentEmail { get; set; }
        // ADDED: Program to link it to the StudentsRegistration table
        public string? Program { get; set; }

        public DateOnly ExamDate { get; set; }
        public bool MailSent { get; set; } = false;

        // REMOVED: public bool IsCompleted { get; set; }
    }
}
