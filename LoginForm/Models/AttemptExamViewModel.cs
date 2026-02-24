using System;
using System.Collections.Generic;

namespace LoginForm.ViewModels
{
    public class AttemptExamViewModel
    {
        // Hidden fields for tracking
        public int ExamId { get; set; }
        public string StudentEmail { get; set; }

        // Display fields
        public string ExamName { get; set; }
        public int DurationMinutes { get; set; }

        // The Questions
        public List<QuestionItem> Questions { get; set; } = new List<QuestionItem>();
    }

    public class QuestionItem
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }

        // The answer selected by the student (bound to Radio Button)
        public string SelectedOption { get; set; }
    }
}