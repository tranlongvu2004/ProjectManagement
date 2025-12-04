using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            _userService = userService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // kiểm tra null
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập email và password";
                return View();
            }

            // kiểm tra hợp lệ
            bool isValid = _userService.IsLoginValid(email, password);

            if (!isValid)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // Lưu session
            HttpContext.Session.SetString("UserEmail", email);

            // Redirect về trang chủ
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
