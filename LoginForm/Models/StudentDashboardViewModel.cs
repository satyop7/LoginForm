using System;
using System.Collections.Generic;

namespace LoginForm.ViewModels
{
    public class StudentDashboardViewModel
    {
        // 1. Personal Information
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Program { get; set; } // e.g., B.Tech
        public string SchoolName { get; set; }
        public string Phone { get; set; }
        public string ProfilePicture { get; set; } // URL or Base64
        public string City { get; set; }

        // 2. Active Exams List
        public List<ActiveExamItem> ActiveExams { get; set; } = new List<ActiveExamItem>();

        // 3. Past Results List
        public List<ResultItem> PastResults { get; set; } = new List<ResultItem>();
    }

    public class ActiveExamItem
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public DateOnly ExamDate { get; set; }
        public string SecureToken { get; set; }
        public bool IsOpenToday { get; set; } // Logic to enable/disable button
    }

    public class ResultItem
    {
        public int TotalQuestions { get; set; }
        public int CorrectlyAnswered { get; set; }
        public bool IsPassed { get; set; }

        // Calculated Property for Percentage
        public double Percentage => TotalQuestions > 0
            ? Math.Round(((double)CorrectlyAnswered / TotalQuestions) * 100, 1)
            : 0;
    }
}