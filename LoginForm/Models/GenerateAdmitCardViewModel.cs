using System;
using System.Collections.Generic;

namespace LoginForm.Models
{
    public class GenerateAdmitCardViewModel
    {
        public List<ProgramAdmitCardItem> PendingAdmitCards { get; set; } = new List<ProgramAdmitCardItem>();
    }

    public class ProgramAdmitCardItem
    {
        public int ExamId { get; set; }
        public string Program { get; set; }
        public DateOnly ExamDate { get; set; }
        public bool IsSelected { get; set; } // Checkbox binds to this
    }
}