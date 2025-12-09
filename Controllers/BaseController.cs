using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult? RedirectIfNotLoggedIn()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            return null;
        }

        // Override OnActionExecuting to auto-load projects for sidebar
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            int? userId = HttpContext.Session.GetInt32("UserId");
            
            if (userId.HasValue)
            {
                var projectServices = HttpContext.RequestServices.GetService<IProjectServices>();
                
                if (projectServices != null)
                {
                    try
                    {
                        var projects = projectServices.GetProjectsOfUserAsync(userId.Value).GetAwaiter().GetResult();
                        ViewBag.Projects = projects;
                    }
                    catch
                    {
                        ViewBag.Projects = new List<ProjectListVM>();
                    }
                }
            }
        }
    }
}
