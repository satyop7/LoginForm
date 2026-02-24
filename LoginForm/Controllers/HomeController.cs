using System.Diagnostics;
using System.IO;                      // Needed for Path and FileStream
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using LoginForm.Models;
using LoginForm.Services;
using LoginForm.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting; // Needed for IWebHostEnvironment
using Microsoft.AspNetCore.Http;    // Needed for IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Required for async DB calls
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace LoginForm.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment; // 1. Inject Environment
        private readonly IEmailService _emailService; // 1. Declare the service
        private readonly IDataProtector _protector;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment environment , IEmailService emailService , IDataProtectionProvider provider)
        {
            _context = context;
            _environment = environment;
            _emailService = emailService;
            _protector = provider.CreateProtector("ExamSecurityToken");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PasswordIncorrect()
        {
            return View();
        }

        public IActionResult ExistingUserFound()
        {
            return View();
        }
        public IActionResult OtpVerification()
        {
            return View();
        }
        //public IActionResult StudentDashboard()
        //{
        //    return View();
        //}
        // 2. Make method Async to handle file I/O

        //Student Dashboard view
        [Authorize(Roles = "Student")] // Protects this page so only students can enter
        public async Task<IActionResult> StudentDashboard()
        {
            // 1. Retrieve the Student ID from the User's "Claims" (The secure cookie)
            var studentIdClaim = User.Claims.FirstOrDefault(c => c.Type == "StudentId");

            if (studentIdClaim == null)
            {
                return RedirectToAction("Login"); // Cookie is missing or invalid
            }

            int studentId = int.Parse(studentIdClaim.Value);

            // 2. Fetch DATA for THIS specific student
            var student = await _context.StudentsRegistration.FindAsync(studentId);

            if (student == null) return NotFound("Student not found");

            // 3. Find ALL Exams assigned to this student's specific PROGRAM
            var programExams = await _context.Exam
                .Where(e => e.Program == student.Program)
                .ToListAsync();

            // 4. Find Results for this STUDENT (to know which exams they already finished)
            var completedExamIds = await _context.Result
                .Where(r => r.StudentEmail == student.Email)
                .Select(r => r.ExamId)
                .ToListAsync();

            // 5. Create the Active Exams list (Filtering out the ones they already took)
            var activeExams = programExams
                .Where(e => !completedExamIds.Contains(e.ExamId)) // Hide completed exams
                .Select(e => new ActiveExamItem
                {
                    ExamId = e.ExamId,
                    ExamName = $"{e.Program} Assessment", // Or e.ExamName if you have a specific column for it
                    ExamDate = e.ExamDate,
                    IsOpenToday = e.ExamDate == DateOnly.FromDateTime(DateTime.Today),
                    // Encrypt the link for the dashboard button
                    SecureToken = HttpUtility.UrlEncode(_protector.Protect($"{e.ExamId}|{student.Email}"))
                })
                .ToList();

            // 6. Fetch detailed Past Results for THIS student to show in the table
            var pastResults = await _context.Result
                .Where(r => r.StudentEmail == student.Email)
                .Select(r => new ResultItem
                {
                    TotalQuestions = r.TotalQuestion,
                    CorrectlyAnswered = r.CorrectlyAnswered,
                    IsPassed = r.IsPassed
                })
                .ToListAsync();

            // 7. Populate ViewModel
            var model = new StudentDashboardViewModel
            {
                FirstName = student.FirstName,
                LastName = student.LastName,
                Email = student.Email,
                Phone = student.Phone,
                Program = student.Program,
                SchoolName = student.SchoolName,
                City = student.City,
                ProfilePicture = student.ProfilePicture,
                ActiveExams = activeExams,     // Uses the filtered list
                PastResults = pastResults      // Uses the results list
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(
        // Inject CaptchaService here if you need it for registration too, otherwise skip
        string firstName, string middleName, string lastName, DateOnly dob, string gender,
        string bloodGroup, string nationality, string category, string govtId,
        string phone, string city, string state, string pincode, string permanentAddress,
        string alternateAddress, string parentName, string parentContactNo,
        string relationWithParent, string program, string previousSchoolName,
        string username, string email, string password, string confirmPassword,
        IFormFile class10Marksheet,
        IFormFile class12Marksheet,
        IFormFile profilePicture
        )
        {
            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Password does not match";
                return View("PasswordIncorrect");
            }

            var existingUser = _context.StudentsRegistration.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
            {
                return View("ExistingUserFound");
            }

            // --- 1. File Upload Logic ---
            string class10Path = await SaveFileAsync(class10Marksheet, "Documents");
            string class12Path = await SaveFileAsync(class12Marksheet, "Documents");
            // --- WATERMARK LOGIC START ---
            IFormFile fileToSave = profilePicture; // Default to original
            MemoryStream watermarkedStream = new MemoryStream();

            if (profilePicture != null && profilePicture.Length > 0)
            {
                using (var imageStream = profilePicture.OpenReadStream())
                using (var image = await Image.LoadAsync(imageStream))
                {
                    // Explicitly tell the compiler to use SixLabors, NOT System.Drawing
                    SixLabors.Fonts.FontFamily fontFamily;
                    if (!SixLabors.Fonts.SystemFonts.TryGet("Arial", out fontFamily))
                        fontFamily = SixLabors.Fonts.SystemFonts.Families.FirstOrDefault();

                    float fontSize = Math.Max(14, image.Width * 0.05f);
                    var font = fontFamily.CreateFont(fontSize, SixLabors.Fonts.FontStyle.Bold);

                    // Setup Watermark Text & Color
                    string watermarkText = "ABC Institute Of Technology";
                    var watermarkColor = Color.FromRgba(255, 255, 255, 180);

                    // Using the modern RichTextOptions as requested
                    var textOptions = new RichTextOptions(font)
                    {
                        Origin = new PointF(image.Width - 10, image.Height - 10),
                        HorizontalAlignment = SixLabors.Fonts.HorizontalAlignment.Right,
                        VerticalAlignment = SixLabors.Fonts.VerticalAlignment.Bottom
                    };

                    // Apply Watermark
                    image.Mutate(ctx => ctx.DrawText(textOptions, watermarkText, watermarkColor));

                    // Save to MemoryStream
                    await image.SaveAsJpegAsync(watermarkedStream);
                    watermarkedStream.Position = 0;

                    // Repackage the stream as an IFormFile
                    fileToSave = new FormFile(watermarkedStream, 0, watermarkedStream.Length, profilePicture.Name, profilePicture.FileName)
                    {
                        Headers = profilePicture.Headers,
                        ContentType = "image/jpeg"
                    };
                }
            }

            // Pass the perfectly mocked IFormFile directly into your existing function!
            string profilePicPath = await SaveFileAsync(fileToSave, "Images");

            // Clean up
            await watermarkedStream.DisposeAsync();
            // --- WATERMARK LOGIC END ---
            // --- 2. Generate OTP Logic ---
            Random random = new Random();
            string otpCode = random.Next(100000, 999999).ToString();

            // --- 3. Create User Object ---
            var newUser = new StudentsRegistration
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DataOfBirth = dob,
                Gender = gender,
                BloodGroup = bloodGroup,
                Nationality = nationality,
                Category = category,
                GovtIdType = govtId,
                Phone = phone,
                City = city,
                State = state,
                Pincode = pincode,
                PermanentAddress = permanentAddress,
                AlternateAddress = alternateAddress,
                ParentName = parentName,
                ParentPhone = parentContactNo,
                Relation = relationWithParent,
                Program = program,
                SchoolName = previousSchoolName,
                Username = username,
                Email = email,
                Password = password,
                Class10Marksheet = class10Path,
                Class12Marksheet = class12Path,
                ProfilePicture = profilePicPath,

                // SAVE OTP TO DATABASE
                OTP = otpCode,
                EmailVerifed = false
            };

            _context.StudentsRegistration.Add(newUser);
            await _context.SaveChangesAsync();

            try

            {

                await _emailService.SendWelcomeEmailAsync(email, firstName, lastName, middleName,

                dob, gender, bloodGroup, nationality, category, govtId, phone,

                city, state, pincode, permanentAddress, alternateAddress, parentName,

                relationWithParent, parentContactNo, program, previousSchoolName, username,

                class10Path, class12Path, profilePicPath);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Email sending failed: " + ex.Message);
            }

            // --- 4. Send Email with OTP ---
            try
            {
                // Update your email service to accept the OTP code
                await _emailService.SendOTPAsync(email, firstName, otpCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email sending failed: " + ex.Message);
            }

            // --- 5. Redirect to Verification Page ---
            // Pass the email so the next page knows who to verify
            return RedirectToAction("OtpVerification", new { email = email });
        }

        // 1. GET: Show the Verification Page
        [HttpGet]
        public IActionResult OtpVerification(string email)
        {
            // Pass the email to the view so we can send it back in the POST request
            ViewBag.Email = email;
            return View();
        }

        // 2. POST: Process the Verification
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp, string captchaToken, [FromServices] CaptchaService _captchaService)
        {
            // A. Verify Captcha FIRST
            bool isCaptchaValid = await _captchaService.VerifyTokenAsync(captchaToken);
            if (!isCaptchaValid)
            {
                ViewBag.ErrorMessage = "Captcha verification failed. Are you a robot?";
                ViewBag.Email = email; // Keep email so they don't lose it
                return View("OtpVerification");
            }

            // B. Find User in DB
            var user = _context.StudentsRegistration.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "User not found.";
                return View("OtpVerification");
            }

            // C. Check if OTP matches
            if (user.OTP == otp)
            {
                // SUCCESS!
                user.EmailVerifed = true;
                user.OTP = null; // Clear OTP after successful use for security
                await _context.SaveChangesAsync();

                await _emailService.SendSuccessEmailAsync(email);
                
                return RedirectToAction("Index"); // Redirect to Homepage/Dashboard
            }
            else
            {
                // FAILURE
                ViewBag.ErrorMessage = "Invalid OTP. Please check your email and try again.";
                ViewBag.Email = email;
                return View("OtpVerification");
            }
        }

        [HttpGet]
        public async Task<IActionResult> AdminDashboard(string? resultEmail)
        {
            // 1. Get Total Count (Async)
            int total = await _context.StudentsRegistration.CountAsync();

            // 2. Get Count per Program
            // Group by 'Program', count them, and convert to Dictionary
            var programData = await _context.StudentsRegistration
                .Where(s => s.Program != null) // Safety check to avoid null keys
                .GroupBy(s => s.Program)
                .Select(g => new { Program = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Program, v => v.Count);

            // 3. Create Model
            var model = new DashboardViewModel
            {
                TotalStudents = total,
                ProgramCounts = programData,
                SearchedResultEmail = resultEmail // Store the searched email
            };

            // --- NEW LOGIC FOR FETCHING RESULTS IN THE DASHBOARD ---
            if (!string.IsNullOrWhiteSpace(resultEmail))
            {
                // 1. Verify student exists
                var studentExists = await _context.StudentsRegistration.AnyAsync(s => s.Email == resultEmail);

                if (!studentExists)
                {
                    TempData["ResultSearchError"] = $"No student found with the email: {resultEmail}";
                }
                else
                {
                    // 2. Fetch the results for the table
                    model.SearchedResults = await _context.Result
                        .Where(r => r.StudentEmail == resultEmail)
                        .ToListAsync();
                }
            }

            // 4. Return View WITH Model
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> StudentDetails(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["SearchError"] = "Please enter a valid email address.";
                return RedirectToAction("AdminDashboard"); // Or whatever your dashboard action is named
            }

            // Find the student by email
            var student = await _context.StudentsRegistration.FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
            {
                TempData["SearchError"] = $"No student found with the email: {email}";
                return RedirectToAction("AdminDashboard");
            }

            return View(student);
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            ClaimsPrincipal principal = null;

            if (role == "Student")
            {
                var student = _context.StudentsRegistration
                    .FirstOrDefault(s => s.Email == email && s.Password == password);

                if (student != null)
                {
                    // Create the "ID Card" (Claims) for the student
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, student.Email),
                new Claim("StudentId", student.RegistrationId.ToString()), // STORE THE ID HERE
                new Claim(ClaimTypes.Role, "Student")
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    principal = new ClaimsPrincipal(identity);
                }
            }
            else if (role == "Admin")
            {
                var admin = _context.AdminLogin
                    .FirstOrDefault(a => a.Email == email && a.Password == password);

                if (admin != null)
                {
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    principal = new ClaimsPrincipal(identity);
                }
            }

            if (principal != null)
            {
                // THIS IS THE KEY STEP: Actually sign the user in via Cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                if (role == "Admin")
                    return RedirectToAction("AdminDashboard"); // Controller name might vary
                else
                    return RedirectToAction("StudentDashboard");
            }
            else
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }
        }
        // GET: Show the form
        [HttpGet]
        public async Task<IActionResult> SetExam()
        {
            var questions = await _context.Question.ToListAsync();

            // Get unique programs from registered students
            var programs = await _context.StudentsRegistration
                .Select(s => s.Program)
                .Distinct()
                .Where(p => !string.IsNullOrEmpty(p))
                .ToListAsync();

            var model = new SetExamViewModel
            {
                ExamDate = DateOnly.FromDateTime(DateTime.Today).AddDays(1),
                AvailablePrograms = programs,
                AvailableQuestions = questions.Select(q => new QuestionSelection
                {
                    QuestionId = q.QuestionId,
                    QuestionDescription = q.QuestionDescription,
                    IsSelected = false
                }).ToList()
            };
            return View(model);
        }

        // POST: Save the exam
        [HttpPost]
        public async Task<IActionResult> SetExam(SetExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Check if at least one question is selected
                var selectedQuestions = model.AvailableQuestions.Where(q => q.IsSelected).ToList();

                if (!selectedQuestions.Any())
                {
                    ModelState.AddModelError("", "You must select at least one question for the exam.");
                    // We'll let it fall through to the reload logic at the bottom
                }
                else
                {
                    // 2. Create the Exam Record (Now linked to a Program)
                    var newExam = new Exam
                    {
                        Program = model.Program, // Changed from StudentEmail
                        ExamDate = model.ExamDate,
                        MailSent = false
                        // Note: IsCompleted is completely removed from here!
                    };

                    _context.Exam.Add(newExam);
                    await _context.SaveChangesAsync(); // Save to generate the new ExamId

                    // 3. Create Links for Selected Questions
                    foreach (var q in selectedQuestions)
                    {
                        var link = new ExamQuestion
                        {
                            ExamId = newExam.ExamId,
                            QuestionId = q.QuestionId
                        };
                        _context.ExamQuestion.Add(link);
                    }

                    await _context.SaveChangesAsync();

                    // 4. Success! Redirect to dashboard
                    TempData["SuccessMessage"] = $"Exam successfully created for the {model.Program} program with {selectedQuestions.Count} questions.";
                    return RedirectToAction("AdminDashboard");
                }
            }

            // IF WE REACH HERE: Validation failed. 
            // We MUST repopulate the AvailablePrograms list before returning the View, 
            // otherwise the dropdown will crash the page because it will be null.
            model.AvailablePrograms = await _context.StudentsRegistration
                .Select(s => s.Program)
                .Distinct()
                .Where(p => !string.IsNullOrEmpty(p))
                .ToListAsync();

            // Keep the questions they already checked as checked
            return View(model);
        }
        //Adding new questions to the database
        [HttpPost]
        public async Task<IActionResult> AddQuestion(Question model)
        {
            if (ModelState.IsValid)
            {
                // Add the new question to the database
                _context.Question.Add(model); // Assuming your DbSet is named 'Question'
                await _context.SaveChangesAsync();

                // Optional: Show a success message
                TempData["SuccessMessage"] = "New question added successfully!";

                // Redirect back to the SetExam page to refresh the question list
                return RedirectToAction("SetExam");
            }

            // If validation fails, return to the SetExam view
            // Note: In a real scenario, you'd want to reload the SetExamViewModel here.
            TempData["ErrorMessage"] = "Failed to add question. Please check the fields.";
            return RedirectToAction("SetExam");
        }

        [HttpGet]
        public async Task<IActionResult> GenerateAdmitCard()
        {
            // Fetch exams (courses) where the bulk admit card emails have NOT been sent yet
            var pendingExams = await _context.Exam
                .Where(e => !e.MailSent)
                .Select(e => new ProgramAdmitCardItem
                {
                    ExamId = e.ExamId,
                    Program = e.Program, // Now mapping to the Program column
                    ExamDate = e.ExamDate,
                    IsSelected = false
                })
                .ToListAsync();

            var model = new GenerateAdmitCardViewModel
            {
                PendingAdmitCards = pendingExams
            };

            return View(model);
        }

        // POST: Send emails to selected students
        [HttpPost]
        public async Task<IActionResult> GenerateAdmitCard(GenerateAdmitCardViewModel model)
        {
            // 1. Filter only the selected exams (which now represent Courses/Programs)
            var selectedExams = model.PendingAdmitCards.Where(x => x.IsSelected).ToList();

            if (!selectedExams.Any())
            {
                ModelState.AddModelError("", "Please select at least one course/exam.");
                return View(model);
            }

            int totalEmailsSent = 0;

            foreach (var item in selectedExams)
            {
                // 2. Fetch the exam details to know which program it belongs to
                var exam = await _context.Exam.FindAsync(item.ExamId);

                if (exam != null && !exam.MailSent)
                {
                    // 3. Fetch ALL students enrolled in this specific Program
                    var enrolledStudents = await _context.StudentsRegistration
                        .Where(s => s.Program == exam.Program)
                        .ToListAsync();

                    // 4. Loop through each student in that program
                    foreach (var student in enrolledStudents)
                    {
                        // B. Generate the PDF dynamically for this specific student
                        byte[] pdfBytes = AdmitCardGenerator.GeneratePdf(student.Email, exam.ExamDate, exam.ExamId);

                        // C. Send the Email
                        // Note: I added 'secureToken' to the end of your parameters so you can use it in your email body!
                        await _emailService.SendEmailWithAttachment(
                            student.Email,
                            pdfBytes,
                            "AdmitCard.pdf",
                            exam.ExamDate,
                            exam.ExamId
                        );

                        totalEmailsSent++;
                    }

                    // 5. Mark the exam as processed so it disappears from the pending list
                    exam.MailSent = true;
                }
            }

            // 6. Save changes to the database
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully sent {totalEmailsSent} admit cards for the selected courses.";
            return RedirectToAction("GenerateAdmitCard");
        }

        // GET: /Exam/Attempt?examId=5&email=student@example.com
        [HttpGet]
        public async Task<IActionResult> Attempt(string token)
        {
            // 1. Check if token exists
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Exam link is missing or invalid.");
            }

            int examId = 0;
            string email = "";

            try
            {
                // 2. Decrypt the secure token
                string decryptedPayload = _protector.Unprotect(token);

                // 3. Extract the ExamId and StudentEmail
                var parts = decryptedPayload.Split('|');
                examId = int.Parse(parts[0]);
                email = parts[1];
            }
            catch (Exception)
            {
                return BadRequest("Invalid, corrupted, or tampered exam link.");
            }

            // 4. Validation A: Does this exam exist?
            var exam = await _context.Exam.FindAsync(examId);

            if (exam == null) return NotFound("Invalid Exam Link. No matching exam found.");

            // 5. Validation B: Is the student authorized for this course's exam?
            var student = await _context.StudentsRegistration.FirstOrDefaultAsync(s => s.Email == email);

            if (student == null || student.Program != exam.Program)
            {
                return Unauthorized("You are not authorized to take this assessment.");
            }

            // 6. Validation C: Has the student ALREADY completed it?
            // We now check the Result table instead of exam.IsCompleted
            bool hasAlreadyCompleted = await _context.Result
                .AnyAsync(r => r.ExamId == examId && r.StudentEmail == email);

            if (hasAlreadyCompleted)
            {
                return Content("You have already attempted and submitted this exam.");
            }

            // 7. Fetch Questions linked to this exam
            var questions = await _context.ExamQuestion
                .Where(eq => eq.ExamId == examId)
                .Select(eq => new QuestionItem
                {
                    QuestionId = eq.Question.QuestionId,
                    Text = eq.Question.QuestionDescription,
                    OptionA = eq.Question.OptionA,
                    OptionB = eq.Question.OptionB,
                    OptionC = eq.Question.OptionC,
                    OptionD = eq.Question.OptionD
                })
                .ToListAsync();

            // 8. Prepare ViewModel
            var model = new AttemptExamViewModel
            {
                ExamId = examId,
                StudentEmail = email,
                ExamName = $"{exam.Program} Assessment", // Dynamically use the Program name
                DurationMinutes = 60, // Or replace with exam.DurationMinutes if you added it to your DB
                Questions = questions
            };

            return View(model);
        }

        // POST: Submit Answers
        [HttpPost]
        public async Task<IActionResult> Submit(AttemptExamViewModel model)
        {
            // 1. Fetch the correct answers from DB to grade them
            // We fetch ONLY the IDs and CorrectOptions to compare
            var questionIds = model.Questions.Select(q => q.QuestionId).ToList();

            var correctAnswers = await _context.Question
                .Where(q => questionIds.Contains(q.QuestionId))
                .ToDictionaryAsync(q => q.QuestionId, q => q.CorrectOption);

            int score = 0;
            int total = model.Questions.Count;

            // 2. Grade the Exam
            foreach (var q in model.Questions)
            {
                if (correctAnswers.ContainsKey(q.QuestionId))
                {
                    // Check if student's selected option matches DB's correct option
                    if (q.SelectedOption == correctAnswers[q.QuestionId])
                    {
                        score++;
                    }
                }
            }

            // 3. Calculate Pass/Fail (Threshold: > 60%)
            double percentage = (double)score / total * 100;
            bool isPassed = percentage > 60;

            // 4. Save to Result Table
            var result = new Result
            {
                ExamId = model.ExamId,
                StudentEmail = model.StudentEmail,
                TotalQuestion = total,
                CorrectlyAnswered = score,
                IsPassed = isPassed
            };
            await _emailService.SendResultEmailAsync(model.StudentEmail, total, score , isPassed);

            _context.Result.Add(result);

            // 5. Mark Exam as Completed
            var exam = await _context.Exam.FindAsync(model.ExamId);

            await _context.SaveChangesAsync();

            // 6. Show Result Page
            return View("ExamResult", result);
        }


        // --- Helper Method to Save Files ---
        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return null; // Handle case where user didn't upload a file
            }

            // Define path: ProjectRoot/Uploads/FolderName (e.g., Uploads/Documents)
            string uploadPath = Path.Combine(_environment.ContentRootPath, "Uploads", folderName);

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename: GUID + OriginalExtension
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(uploadPath, uniqueFileName);

            // Save to disk
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Return the filename (or relative path) to store in DB
            return uniqueFileName;
        }
    }
}