using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service;
using PorjectManagement.Service.Interface;
using System.Net.Mail;
namespace PorjectManagement.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            _userService = userService;
        }

        // Login 
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
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập email và password";
                return View();
            }
            bool isValid = _userService.IsLoginValid(email, password);

            if (!isValid)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng.";
                return View();
            }
            var user = _userService.GetUser(email);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                return View();
            }
            
            // Lưu cả UserId VÀ RoleId
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetInt32("RoleId", user.RoleId);
            HttpContext.Session.SetInt32("UserId", user.UserId);  
    
            if (user.RoleId == 1)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (user.RoleId == 2)
            {
                return RedirectToAction("Index", "Home");
            }
            else if (user.RoleId == 3)
            {
                return RedirectToAction("Index", "Project");
            }
            return RedirectToAction("Index", "Home");
        }


        // Register
        public IActionResult Register()
        {
            return View();
        }
        bool IsValidEmail(string email)
        {
            try
            {
                var mail = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }
        [HttpPost]
        public IActionResult Register(string fullName, string email, string password, string confirmpassword)
        {
            if (!IsValidEmail(email))
            {
                ViewBag.Error = "Email không đúng định dạng.";
                return View();
            }

            if (password != confirmpassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }
            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }
            if (password.Length <= 4 && confirmpassword.Length <= 4)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 4 ký tự.";
                return View();
            }
            var existUser = _userService.GetUser(email);
            if (existUser != null)
            {
                ViewBag.Error = "Email đã tồn tại.";
                return View();
            }
            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = password,   
                RoleId = 3,                
                Status = UserStatus.Active,
                CreatedAt = DateTime.Now
            };
            _userService.CreateAccount(newUser);
            return RedirectToAction("Login");
        }


        // Reset password
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            var user = _userService.GetUser(email);
            if (user == null)
            {
                ViewBag.Error = "Email không tồn tại.";
                return View();
            }

            return RedirectToAction("ResetPasswordConfirm", new { email = email });
        }


        public IActionResult ResetPasswordConfirm(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPasswordConfirm(string email, string newpassword, string confirmpassword)
        {
            if (string.IsNullOrEmpty(newpassword) || string.IsNullOrEmpty(confirmpassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ.";
                ViewBag.Email = email;
                return View();
            }

            if (newpassword != confirmpassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                ViewBag.Email = email;
                return View();
            }
            if (newpassword.Length <= 4 && confirmpassword.Length <= 4)
            {
                ViewBag.Error = "Mật khẩu phải có ít nhất 4 ký tự.";
                return View();
            }
            var user = _userService.GetUser(email);
            if (user == null)
            {
                ViewBag.Error = "Không tìm thấy tài khoản.";
                return View();
            }
            user.PasswordHash = newpassword;
            _userService.UpdateUser(user);

            return RedirectToAction("Login");
        }


        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
