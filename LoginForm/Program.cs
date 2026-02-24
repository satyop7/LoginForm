using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using LoginForm.Models;
using LoginForm.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add this near your DbContext registration
builder.Services.AddTransient<LoginForm.Services.IEmailService, LoginForm.Services.EmailService>();
// Register CaptchaService and HttpClient
builder.Services.AddHttpClient<LoginForm.Services.CaptchaService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login"; // Redirect here if not logged in
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

string imagesPath = @"F:\StudentRegistrationForm\LoginForm\LoginForm\Uploads\Images";
string documentsPath = @"F:\StudentRegistrationForm\LoginForm\LoginForm\Uploads\Documents";

// Ensure the directory exists to avoid crashes
if (Directory.Exists(imagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(imagesPath),
        RequestPath = "/MyImages" // This is the URL prefix you will use in HTML
    });
}
if (Directory.Exists(documentsPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(documentsPath),
        RequestPath = "/MyDocuments" // This is the URL prefix you will use in HTML
    });
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
