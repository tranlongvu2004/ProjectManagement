using NuGet.Packaging;
using OfficeOpenXml;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Repository;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service;
using PorjectManagement.Service.Interface;
using System.ComponentModel;


var builder = WebApplication.CreateBuilder(args);
ExcelPackage.License.SetNonCommercialPersonal("Task Lab");
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSqlServer<LabProjectManagementContext>(builder.Configuration.GetConnectionString("MyCnn"));
builder.Services.AddSession();
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/User/Login";
    });

builder.Services.AddAuthorization();

// Dependency Injection for Repositories and Services
builder.Services.AddScoped<IUserRepo,UserRepo>();
builder.Services.AddScoped<IUserServices,UserServices>();
builder.Services.AddScoped<IProjectRepo, ProjectRepo>();
builder.Services.AddScoped<IProjectServices, ProjectServices>();

builder.Services.AddScoped<IUserProjectRepo, UserProjectRepo>();
builder.Services.AddScoped<IUserProjectService, UserProjectService>();

builder.Services.AddScoped<ITaskRepo, TaskRepo>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IInternRepo, InternRepo>();
builder.Services.AddScoped<IInternService, InternService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<IEmailRepo, EmailRepo>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<EmailSender>();
builder.Services.AddHostedService<DeadlineEmailBackgroundService>();

builder.Services.AddSignalR();
builder.Services.AddScoped<ICommentService, CommentService>(); 


var app = builder.Build();
app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.MapHub<TaskHub>("/taskHub");

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();