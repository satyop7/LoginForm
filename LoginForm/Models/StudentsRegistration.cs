using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoginForm.Models
{
    [Table("StudentsRegistration")]
    public class StudentsRegistration
    {
        [Key]
        public int RegistrationId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public DateOnly DataOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? Nationality { get; set; }
        public string? Category { get; set; }
        public string? GovtIdType { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? PermanentAddress { get; set; }
        public string? AlternateAddress { get; set; }
        public string? ParentName { get; set; }
        public string? ParentPhone { get; set; }
        public string? Relation { get; set; }
        public string? Program { get; set; }
        public string? SchoolName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Class10Marksheet { get; set; }
        public string? Class12Marksheet { get; set; }
        public string? ProfilePicture { get; set; }
        public bool EmailVerifed { get; set; } = false;
        public string? OTP { get; set; }
    }
}
