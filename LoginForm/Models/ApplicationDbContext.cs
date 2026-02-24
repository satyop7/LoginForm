using Microsoft.EntityFrameworkCore;

namespace LoginForm.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<StudentsRegistration> StudentsRegistration { get; set; }
        public DbSet<AdminLogin> AdminLogin { get; set; }
        public DbSet<Exam> Exam { get; set; }
        public DbSet<Question> Question { get; set; }
        public DbSet<ExamQuestion> ExamQuestion { get; set; }
        public DbSet<Result> Result { get; set; }



    }
}
