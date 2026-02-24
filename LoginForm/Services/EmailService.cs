using System.Buffers.Text;
using System.Net;
using System.Net.Mail;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.DataProtection;
using System.Web; // Needed for UrlEncode


namespace LoginForm.Services
{
    // 1. Create an Interface (Best practice for Dependency Injection)
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string firstName, string lastName, string middleName,
            DateOnly dob, string gender, string bloodGroup, string nationality, string category, string govtId, string phone,
            string city, string state, string pincode, string permanentAddress, string alternateAddress, string parentName,
            string relationWithParent, string parentContactNo, string program, string previousSchoolName, string username,
            string class10Path, string class12Path, string profilePicPath);

        Task SendOTPAsync(string toEmail, string firstName, string otp);

        Task SendSuccessEmailAsync(string toEmail);
        Task SendEmailWithAttachment(string toEmail, byte[] pdfData, string fileName , DateOnly examDate, int examId);
        Task SendResultEmailAsync(string toEmail, int totalQuestions, int correctAnswers, bool isPassed);
    }

    // 2. Implement the Interface
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly IDataProtector _protector;

        public EmailService(IConfiguration config , IDataProtectionProvider provider)
        {
            _config = config; // Injects appsettings.json
            _protector = provider.CreateProtector("ExamSecurityToken");
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string firstName , string lastName , string middleName ,
            DateOnly dob , string gender , string bloodGroup , string nationality , string category , string govtId , string phone ,
            string city , string state , string pincode , string permanentAddress , string alternateAddress , string parentName ,
            string relationWithParent , string parentContactNo , string program , string previousSchoolName , string username ,
            string class10Path , string class12Path , string profilePicPath)
        {
            var server = _config["SmtpSettings:Server"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var senderName = _config["SmtpSettings:SenderName"];
            var password = _config["SmtpSettings:Password"];

            var smtpClient = new SmtpClient(server)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true,
            };

            // 1. Build the HTML string using string interpolation
            string emailBody = $@"
<div style='font-family: Arial, sans-serif; color: #333; max-width: 650px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 8px rgba(0,0,0,0.05);'>
    
    <div style='background-color: #004085; color: #ffffff; padding: 20px; text-align: center;'>
        <h2 style='margin: 0; font-size: 24px;'>Registration Successful!</h2>
    </div>
    
    <div style='padding: 25px;'>
        <p style='font-size: 16px;'>Dear <strong>{firstName} {lastName}</strong>,</p>
        <p style='line-height: 1.6;'>Thank you for filling out the registration form. Your application has been successfully saved. Below is a summary of the details you submitted:</p>
        
        <table style='width: 100%; border-collapse: collapse; margin-top: 20px; font-size: 14px;'>
            
            <tr>
                <th colspan='2' style='background-color: #f8f9fa; padding: 12px; text-align: left; border: 1px solid #dee2e6; color: #004085;'>Personal Details</th>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; width: 35%; font-weight: bold;'>Full Name</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{firstName} {middleName} {lastName}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Date of Birth</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{dob}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Gender & Blood Group</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{gender} | {bloodGroup}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Nationality / Category</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{nationality} / {category}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Govt ID Type</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{govtId}</td>
            </tr>

            <tr>
                <th colspan='2' style='background-color: #f8f9fa; padding: 12px; text-align: left; border: 1px solid #dee2e6; color: #004085; border-top: 2px solid #dee2e6;'>Contact Details</th>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Email</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'><a href='mailto:{toEmail}' style='color: #0056b3;'>{toEmail}</a></td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Phone</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{phone}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>City / State / Pincode</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{city}, {state} - {pincode}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Permanent Address</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{permanentAddress}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Alternate Address</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{(string.IsNullOrWhiteSpace(alternateAddress) ? "N/A" : alternateAddress)}</td>
            </tr>

            <tr>
                <th colspan='2' style='background-color: #f8f9fa; padding: 12px; text-align: left; border: 1px solid #dee2e6; color: #004085; border-top: 2px solid #dee2e6;'>Guardian & Academic Info</th>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Guardian Name</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{parentName} ({relationWithParent})</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Guardian Contact</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{parentContactNo}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Program Applied</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{program}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Previous School</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{previousSchoolName}</td>
            </tr>

            <tr>
                <th colspan='2' style='background-color: #f8f9fa; padding: 12px; text-align: left; border: 1px solid #dee2e6; color: #004085; border-top: 2px solid #dee2e6;'>Account & Uploads</th>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Username</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>{username}</td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Password</td>
                <td style='padding: 10px; border: 1px solid #dee2e6; color: green;'><em>[Hidden for Security]</em></td>
            </tr>
            <tr>
                <td style='padding: 10px; border: 1px solid #dee2e6; font-weight: bold;'>Documents Uploaded</td>
                <td style='padding: 10px; border: 1px solid #dee2e6;'>
                    Class 10: {(string.IsNullOrEmpty(class10Path) ? "❌" : "✅")}<br/>
                    Class 12: {(string.IsNullOrEmpty(class12Path) ? "❌" : "✅")}<br/>
                    Profile Pic: {(string.IsNullOrEmpty(profilePicPath) ? "❌" : "✅")}
                </td>
            </tr>
        </table>
        
        <p style='margin-top: 25px; line-height: 1.6;'>If you notice any discrepancies in the information above, please contact the administration department immediately.</p>
        
        <p style='margin-top: 30px; font-size: 14px; color: #555;'>
            Best Regards,<br/>
            <strong>The Admissions Team</strong>
        </p>
    </div>
</div>";

            // 2. Assign the string to the MailMessage
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Registration Successful! Welcome aboard.",
                Body = emailBody, // Attach the HTML string here
                IsBodyHtml = true // CRITICAL: This tells the email client to render the HTML
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendOTPAsync(string toEmail, string firstName, string otp)
        {
            var server = _config["SmtpSettings:Server"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var senderName = _config["SmtpSettings:SenderName"];
            var password = _config["SmtpSettings:Password"];

            var smtpClient = new SmtpClient(server)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true,
            };

            // Build the HTML email body for OTP
            string emailBody = $@"
<div style='font-family: Arial, sans-serif; color: #333; max-width: 500px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 8px rgba(0,0,0,0.05);'>

    <div style='background-color: #28a745; color: #ffffff; padding: 20px; text-align: center;'>
        <h2 style='margin: 0; font-size: 24px;'>Email Verification</h2>
    </div>

    <div style='padding: 30px; text-align: center;'>
        <p style='font-size: 16px; margin-bottom: 10px;'>Hello <strong>{firstName}</strong>,</p>
        <p style='font-size: 14px; color: #666; line-height: 1.6; margin-bottom: 25px;'>
            Your One-Time Password (OTP) for email verification is:
        </p>

        <div style='background-color: #f8f9fa; border: 2px dashed #28a745; padding: 20px; border-radius: 6px; margin: 20px 0;'>
            <p style='font-size: 32px; font-weight: bold; color: #28a745; margin: 0; letter-spacing: 3px;'>{otp}</p>
        </div>

        <p style='font-size: 14px; color: #666; margin: 20px 0;'>
            This OTP is valid for <strong>10 minutes</strong> only.
        </p>

        <p style='font-size: 13px; color: #999; line-height: 1.6; margin-top: 25px;'>
            <strong>Security Note:</strong> Never share this OTP with anyone. The admissions team will never ask you for this code.
        </p>

        <hr style='border: none; border-top: 1px solid #dee2e6; margin: 25px 0;' />

        <p style='font-size: 12px; color: #999; margin: 10px 0;'>
            If you did not request this verification, please ignore this email.
        </p>

        <p style='font-size: 12px; color: #999;'>
            Best Regards,<br/>
            <strong>The Admissions Team</strong>
        </p>
    </div>
</div>";

            // Create and send the mail message
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Your OTP for Email Verification",
                Body = emailBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendSuccessEmailAsync(string toEmail)
        {
            var server = _config["SmtpSettings:Server"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var senderName = _config["SmtpSettings:SenderName"];
            var password = _config["SmtpSettings:Password"];

            var smtpClient = new SmtpClient(server)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true,
            };

            // Build the HTML email body for email verification success
            string emailBody = $@"
<div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 8px rgba(0,0,0,0.05);'>

    <div style='background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: #ffffff; padding: 30px; text-align: center;'>
        <h2 style='margin: 0; font-size: 28px;'>✓ Email Verified Successfully!</h2>
    </div>

    <div style='padding: 40px;'>
        <p style='font-size: 16px; line-height: 1.8; margin-bottom: 20px;'>
            Congratulations! Your email has been successfully verified.
        </p>

        <div style='background-color: #f0f8f5; border-left: 4px solid #28a745; padding: 20px; border-radius: 4px; margin: 25px 0;'>
            <p style='font-size: 15px; color: #155724; margin: 0; line-height: 1.6;'>
                <strong>✓ Email Verification:</strong> Your email address has been confirmed and is now active on your account.
            </p>
        </div>

        <p style='font-size: 14px; color: #666; line-height: 1.8; margin-top: 25px;'>
            You can now proceed with completing your registration. Your account is ready to use, and you have full access to all features.
        </p>

        <p style='font-size: 14px; color: #666; line-height: 1.8; margin-top: 20px;'>
            If you did not perform this action or have any concerns about your account security, please contact the admissions team immediately.
        </p>

        <div style='background-color: #f9f9f9; padding: 20px; border-radius: 6px; margin-top: 30px; text-align: center;'>
            <p style='font-size: 13px; color: #666; margin: 0;'>
                <strong>What's Next?</strong><br/>
                Complete your registration and submit all required documents to finalize your admission process.
            </p>
        </div>

        <hr style='border: none; border-top: 1px solid #dee2e6; margin: 30px 0;' />

        <p style='font-size: 12px; color: #999; margin-top: 20px;'>
            Thank you for verifying your email address. We're excited to have you as part of our institution!
        </p>

        <p style='font-size: 12px; color: #999; margin-top: 15px;'>
            Best Regards,<br/>
            <strong>The Admissions Team</strong>
        </p>

        <p style='font-size: 11px; color: #bbb; margin-top: 20px; text-align: center;'>
            This is an automated email. Please do not reply to this message.
        </p>
    </div>
</div>";

            // Create and send the mail message
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = "Email Verified Successfully! ✓",
                Body = emailBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        public async Task SendEmailWithAttachment(string toEmail, byte[] pdfData, string fileName , DateOnly examDate , int examId)
        {
            var server = _config["SmtpSettings:Server"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var senderName = _config["SmtpSettings:SenderName"];
            var password = _config["SmtpSettings:Password"];

            var smtpClient = new SmtpClient(server)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true,
            };

            // 1. Combine the data you want to hide into one string separated by a pipe (|)
            string secretPayload = $"{examId}|{toEmail}";

            // 2. Encrypt the string
            string encryptedToken = _protector.Protect(secretPayload);

            // 3. Make the token safe to put in a URL (changes + to %2B, etc.)
            string urlSafeToken = HttpUtility.UrlEncode(encryptedToken);

            // 1. Set your domain (Change the port number to match your running app)
            string baseUrl = "https://localhost:5254";

            // 2. Create the full URL
            string examUrl = $"{baseUrl}/Home/Attempt?token={urlSafeToken}";
            string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        /* Fallback for clients that support style tags */
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f0fff4; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: #38a169; padding: 30px; text-align: center; color: white; }}
        .content {{ padding: 40px 30px; color: #2d3748; line-height: 1.6; }}
        .exam-info {{ background-color: #f0fff4; border-left: 4px solid #38a169; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ background-color: #f7fafc; padding: 20px; text-align: center; font-size: 12px; color: #718096; border-top: 1px solid #e2e8f0; }}
        .btn {{ display: inline-block; padding: 12px 24px; background-color: #38a169; color: white; text-decoration: none; border-radius: 6px; font-weight: bold; margin-top: 20px; }}
    </style>
</head>
<body style='background-color: #f0fff4; font-family: sans-serif;'>
    
    <div style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        
        <div style='background-color: #38a169; padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 24px;'>Exam Admit Card</h1>
        </div>

        <div style='padding: 40px 30px; color: #2d3748;'>
            <p style='font-size: 16px; margin-top: 0;'>Dear Student,</p>
            
            <p>Your examination has been officially scheduled. We are pleased to inform you that your <strong>Admit Card</strong> has been generated and is attached to this email.</p>
            
            <div style='background-color: #f0fff4; border-left: 5px solid #38a169; padding: 15px; margin: 25px 0;'>
                <p style='margin: 5px 0; font-weight: bold; color: #22543d;'>📅 Exam Date: {examDate}</p>
                <p style='margin: 5px 0; color: #22543d;'>🆔 Exam ID: #{examId}</p>
                <p style='margin: 5px 0; color: #22543d;'>📧 Registered Email: {toEmail}</p>
                <a href='{examUrl}' style='color: #38a169; text-decoration: underline;'>Click Here to Start Exam</a>
            </div>

            <p>Please download and print the attached PDF. You will need to present this document or use the credentials inside it to access the examination portal.</p>

            <p><strong>Important Instructions:</strong></p>
            <ul style='color: #4a5568; padding-left: 20px;'>
                <li>Ensure you have a stable internet connection.</li>
                <li>Login 15 minutes prior to the scheduled time.</li>
                <li>Do not share your exam credentials.</li>
            </ul>

            <p style='margin-top: 30px;'>Good luck with your preparation!</p>
            <p style='margin-bottom: 0;'>Best Regards,<br><strong>Examination Controller</strong></p>
        </div>

        <div style='background-color: #f7fafc; padding: 20px; text-align: center; font-size: 12px; color: #718096; border-top: 1px solid #e2e8f0;'>
            <p style='margin: 0;'>This is an automated message. Please do not reply to this email.</p>
            <p style='margin: 5px 0;'>© {DateTime.Now.Year} Your Institute Name. All rights reserved.</p>
        </div>

    </div>
</body>
</html>";
            // Create and send the mail message
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "Exam Portal"),
                Subject = "Email Verified Successfully! ✓",
                Body = emailBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            // ATTACH THE PDF
            using (var stream = new MemoryStream(pdfData))
            {
                var attachment = new Attachment(stream, fileName, "application/pdf");
                mailMessage.Attachments.Add(attachment);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
        public async Task SendResultEmailAsync(string toEmail, int totalQuestions, int correctAnswers, bool isPassed)
        {
            // 1. Calculate Score Details
            double percentage = totalQuestions > 0 ? ((double)correctAnswers / totalQuestions) * 100 : 0;
            string percentageString = percentage.ToString("0.##") + "%";

            // 2. Set Theme Colors based on Pass/Fail status
            string statusColor = isPassed ? "#38a169" : "#e53e3e"; // Green or Red
            string statusText = isPassed ? "PASSED" : "FAILED";
            string subText = isPassed ? "Congratulations! You have successfully cleared the exam."
                                      : "Unfortunately, you did not meet the passing criteria.";

            // 3. Construct the HTML Body
            string emailBody = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <style>
            body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 0; padding: 0; background-color: #f0fff4; }}
            .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
            .header {{ background-color: #38a169; padding: 30px; text-align: center; color: white; }}
            .content {{ padding: 40px 30px; color: #2d3748; text-align: center; }}
            .score-card {{ background-color: #f7fafc; border-radius: 8px; padding: 20px; margin: 20px 0; border: 1px solid #e2e8f0; }}
            .score-large {{ font-size: 3rem; font-weight: bold; color: {statusColor}; margin: 10px 0; }}
            .status-badge {{ background-color: {statusColor}; color: white; padding: 5px 15px; border-radius: 20px; font-weight: bold; font-size: 0.9rem; letter-spacing: 1px; }}
            .footer {{ background-color: #f0fff4; padding: 20px; text-align: center; font-size: 12px; color: #718096; border-top: 1px solid #e2e8f0; }}
            .stat-row {{ display: flex; justify-content: space-between; margin-bottom: 10px; border-bottom: 1px solid #edf2f7; padding-bottom: 5px; }}
            .stat-label {{ color: #718096; }}
            .stat-value {{ font-weight: bold; color: #2d3748; }}
        </style>
    </head>
    <body style='margin: 0; padding: 0; background-color: #f0fff4; font-family: sans-serif;'>
        <div class='container' style='max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05);'>
            
            <div class='header' style='background-color: #38a169; padding: 30px; text-align: center; color: white;'>
                <h1 style='margin: 0; font-size: 24px;'>Exam Results Declared</h1>
            </div>

            <div class='content' style='padding: 40px 30px; color: #2d3748; text-align: center;'>
                <p style='font-size: 18px; margin-top: 0;'>Hello Student,</p>
                <p>{subText}</p>

                <div class='score-card' style='background-color: #f7fafc; border-radius: 8px; padding: 25px; margin: 25px 0; border: 1px solid #e2e8f0;'>
                    <span class='status-badge' style='background-color: {statusColor}; color: white; padding: 6px 16px; border-radius: 20px; font-weight: bold; font-size: 14px; letter-spacing: 1px;'>{statusText}</span>
                    
                    <div class='score-large' style='font-size: 48px; font-weight: bold; color: {statusColor}; margin: 15px 0;'>
                        {percentageString}
                    </div>
                    
                    <div style='text-align: left; margin-top: 20px;'>
                        <div style='display: flex; justify-content: space-between; border-bottom: 1px solid #e2e8f0; padding: 8px 0;'>
                            <span style='color: #718096;'>Total Questions:</span>
                            <span style='font-weight: bold;'>{totalQuestions}</span>
                        </div>
                        <div style='display: flex; justify-content: space-between; border-bottom: 1px solid #e2e8f0; padding: 8px 0;'>
                            <span style='color: #718096;'>Correct Answers:</span>
                            <span style='font-weight: bold;'>{correctAnswers}</span>
                        </div>
                         <div style='display: flex; justify-content: space-between; padding: 8px 0;'>
                            <span style='color: #718096;'>Wrong Answers:</span>
                            <span style='font-weight: bold;'>{totalQuestions - correctAnswers}</span>
                        </div>
                    </div>
                </div>

                <p style='color: #718096; font-size: 0.9em;'>This result is generated automatically based on your submission.</p>
                
                <a href='#' style='display: inline-block; margin-top: 15px; text-decoration: none; color: #38a169; font-weight: bold;'>Login to Portal for Details &rarr;</a>
            </div>

            <div class='footer' style='background-color: #f0fff4; padding: 20px; text-align: center; font-size: 12px; color: #718096; border-top: 1px solid #e2e8f0;'>
                &copy; {DateTime.Now.Year} Exam Portal. All rights reserved.
            </div>
        </div>
    </body>
    </html>";

            // 4. Configure SMTP Settings
            var server = _config["SmtpSettings:Server"];
            var port = int.Parse(_config["SmtpSettings:Port"]);
            var senderEmail = _config["SmtpSettings:SenderEmail"];
            var senderName = _config["SmtpSettings:SenderName"];
            var password = _config["SmtpSettings:Password"];

            using (var smtpClient = new SmtpClient(server))
            {
                smtpClient.Port = port;
                smtpClient.Credentials = new NetworkCredential(senderEmail, password);
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = isPassed ? "Exam Result: Congratulations!" : "Exam Result: Update",
                    Body = emailBody,
                    IsBodyHtml = true // IMPORTANT: Renders the HTML
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

    }
}
