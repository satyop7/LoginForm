namespace LoginForm.Models
{
    // Models/DashboardViewModel.cs
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }

        // This holds the program name and the count of students in it
        // e.g., { "Computer Science": 120, "Civil": 45 }
        public Dictionary<string, int> ProgramCounts { get; set; } = new Dictionary<string, int>();

        public string? SearchedResultEmail { get; set; }
        public List<Result>? SearchedResults { get; set; }
    }
}
