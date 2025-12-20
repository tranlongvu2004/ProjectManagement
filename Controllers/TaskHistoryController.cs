using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class TaskHistoryController : Controller
    {
        private readonly Service.Interface.ITaskHistoryService _taskHistoryService;

        public TaskHistoryController(ITaskHistoryService taskHistoryService)
        {
            _taskHistoryService = taskHistoryService;
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var histories = await _taskHistoryService.GetAllAsync();

            var vm = histories.Select(h => new TaskHistoryVM
            {
                UserName = h.User.FullName,
                Action = h.Action,
                Description = h.Description,
                CreatedAt = h.CreatedAt
            }).ToList();

            return View(vm);
        }


    }
}
