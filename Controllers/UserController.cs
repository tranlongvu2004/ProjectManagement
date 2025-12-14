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
            ViewBag.Email = email;
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter email and password";
                return View();
            }
            bool isValid = _userService.IsLoginValid(email, password);

            if (!isValid)
            {
                ViewBag.Error = "Email or password is incorrect.";
                return View();
            }
            var user = _userService.GetUser(email);
            if (user == null)
            {
                ViewBag.Error = "Account not found.";
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
            ViewBag.FullName = fullName;
            ViewBag.Email = email;
            if (!IsValidEmail(email))
            {
                ViewBag.Error = "Email is invalid format.";
                return View();
            }

            if (password != confirmpassword)
            {
                ViewBag.Error = "Password missmatch.";
                return View();
            }
            if (string.IsNullOrEmpty(fullName) ||
                string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter all the field.";
                return View();
            }
            if (password.Length <= 4 && confirmpassword.Length <= 4)
            {
                ViewBag.Error = "Password must have at least 4 character.";
                return View();
            }
            var existUser = _userService.GetUser(email);
            if (existUser != null)
            {
                ViewBag.Error = "Email existed.";
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
                ViewBag.Error = "Please enter email.";
                return View();
            }

            var user = _userService.GetUser(email);
            if (user == null)
            {
                ViewBag.Error = "Email does not exist.";
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
                ViewBag.Error = "Please enter all the field.";
                ViewBag.Email = email;
                return View();
            }

            if (newpassword != confirmpassword)
            {
                ViewBag.Error = "Password missmatch.";
                ViewBag.Email = email;
                return View();
            }
            if (newpassword.Length <= 4 && confirmpassword.Length <= 4)
            {
                ViewBag.Error = "Password must have at least 4 character.";
                return View();
            }
            var user = _userService.GetUser(email);
            if (user == null)
            {
                ViewBag.Error = "Account not found.";
                return View();
            }
            user.PasswordHash = newpassword;
            _userService.UpdateUser(user);

            return RedirectToAction("Login");
        }

        //Change Password
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            return RedirectToAction("/User/ResetPassword");
        }



        //Change Password
        [HttpGet]
        public IActionResult Profile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var user = _userService.GetUserById(userId.Value);
            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(string fullName, string email, IFormFile? avatarFile)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _userService.GetUserById(userId.Value);

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Please enter all the field.";
                return View(user);
            }

            // Upload avatar
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var fileName = $"{Guid.NewGuid()}_{avatarFile.FileName}";
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars", fileName);

                using var stream = new FileStream(savePath, FileMode.Create);
                avatarFile.CopyTo(stream);

                user.AvatarUrl = "/avatars/" + fileName;
            }

            user.FullName = fullName;
            user.Email = email;

            _userService.UpdateProfile(user);

            ViewBag.Message = "Update profile successfully!";
            return View(user);
        }

        [HttpGet]
        public IActionResult ViewProfile(int userid)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            var user = _userService.GetUserById(userid);
            return View(user);
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
