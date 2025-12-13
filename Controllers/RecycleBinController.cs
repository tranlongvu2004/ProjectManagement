using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class RecycleBinController : Controller
    {
        private readonly LabProjectManagementContext _context;
        public RecycleBinController(LabProjectManagementContext context)
        {
            _context = context;
        }
        public IActionResult RecycleBin()
        {
            var data = _context.RecycleBins
    .Where(rb => rb.EntityType == "Task")
    .OrderByDescending(rb => rb.DeletedAt)
    .Select(x => new
    {
        Recycle = x,
        DeletedUser = _context.Users.FirstOrDefault(u => u.UserId == x.DeletedBy)
    })
    .AsEnumerable()
    .Select(x =>
    {
        var task = System.Text.Json.JsonSerializer.Deserialize<DTOTaskSnapshot>(x.Recycle.DataSnapshot);

        return new RecyclebinVM
        {
            RecycleId = x.Recycle.RecycleId,
            EntityType = "Task",
            Name = task?.TaskName ?? "(Unknown Task)",
            Owner = task?.Owner ?? "Unassigned",
            Status = task?.Status ?? "Unknown",
            DeletedBy = x.DeletedUser?.FullName ?? "Unknown",
            DeletedAt = x.Recycle.DeletedAt ?? DateTime.MinValue
        };
    })
    .ToList();



            return View(data);
        }
    }
}
