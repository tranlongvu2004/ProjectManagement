using PorjectManagement.Models;
using PorjectManagement.Repository;
using PorjectManagement.Repository.Interface;
using PorjectManagement.Service;
using PorjectManagement.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSqlServer<LabProjectManagementContext>(builder.Configuration.GetConnectionString("MyCnn"));
builder.Services.AddSession();

// Dependency Injection for Repositories and Services
builder.Services.AddScoped<IUserRepo,UserRepo>();
builder.Services.AddScoped<IUserServices,UserServices>();

var app = builder.Build();
app.UseSession();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Dashboard}/{id?}");

app.Run();
