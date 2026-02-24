using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LoginForm.Models
{
    public class SetExamViewModel
    {
        [Required]
        [Display(Name = "Select Program")]
        public string Program { get; set; } // Replaced StudentEmail

        [Required]
        [Display(Name = "Exam Date")]
        public DateOnly ExamDate { get; set; }
            
        // --- Question Selection ---
        public List<QuestionSelection> AvailableQuestions { get; set; } = new List<QuestionSelection>();

        // To populate the dropdown
        public List<string> AvailablePrograms { get; set; } = new List<string>();
    }

    public class QuestionSelection
    {
        public int QuestionId { get; set; }
        public string QuestionDescription { get; set; }
        public bool IsSelected { get; set; } // The checkbox binds to this
    }
}