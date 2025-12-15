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
        private readonly IEmailService _emailService;

        public UserController(IUserServices userService, IEmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
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
        public IActionResult Register(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Email is required";
                return View();
            }

            if (_userService.GetUser(email) != null)
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            var user = new User
            {
                Email = email,
                FullName = "", // tạm
                PasswordHash = "",
                RoleId = 2,
                Status = UserStatus.Inactive,
                CreatedAt = DateTime.Now
            };

            _userService.CreateAccount(user);

            var token = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(email)
            );

            var link = Url.Action(
                "CompleteRegister",
                "User",
                new { email = email, token = token },
                Request.Scheme
            );

            _emailService.Send(email, "Confirm email", link);

            return View("CheckEmail");
        }

        public IActionResult CompleteRegister(string email, string token)
        {
            var decode = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(token)
            );

            if (decode != email)
                return View("InvalidLink");

            var user = _userService.GetUser(email);
            if (user.Status == UserStatus.Inactive && user.CreatedAt.HasValue && user.CreatedAt.Value.AddMinutes(30) < DateTime.Now)
            {
                _userService.DeleteUser(email);
                return View("InvalidLink");
            }
            if (user == null || user.Status != UserStatus.Inactive)
                return View("InvalidLink");
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        public IActionResult CompleteRegister(string email,string fullName,string password,string confirmPassword)
        {
            ViewBag.fullName = fullName;
            ViewBag.email = email;
            var user = _userService.GetUser(email);
            if (password.Length <= 4 || confirmPassword.Length <= 4)
            {
                ViewBag.Error = "Password must have at least 5 character.";
                return View();
            }
            if (user == null)
                return View("InvalidLink");

            if (password != confirmPassword)
            {
                ViewBag.Error = "Password Mismatch";
                return View();
            }

            user.FullName = fullName;
            user.PasswordHash = password;
            user.Status = UserStatus.Active;

            _userService.UpdateUser(user);
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

            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(email + "|" + DateTime.Now));        

            var resetLink = Url.Action(
                "ResetPasswordConfirm",
                "User",
                new { token = token },
                Request.Scheme
            );

            _emailService.Send(
                email,
                "Reset your password",
                $"Click here to reset password: {resetLink}"
            );

            ViewBag.Message = "Reset password link has been sent to your email.";
            return View();
        }



        public IActionResult ResetPasswordConfirm(string token)
        {
            try
            {
                var decoded = System.Text.Encoding.UTF8.GetString(
                    Convert.FromBase64String(token)
                );
                var parts = decoded.Split('|');
                var email = parts[0];
                var createdTime = DateTime.Parse(parts[1]);
                if (createdTime.AddMinutes(2) < DateTime.Now)
                    return View("InvalidLink");

                var user = _userService.GetUser(email);
                if (user == null)
                    return View("InvalidLink");

                ViewBag.Email = email;
                ViewBag.Token = token;
                return View();
            }
            catch
            {
                return View("InvalidLink");
            }
        }


        [HttpPost]
        public IActionResult ResetPasswordConfirm(string email, string token, string newpassword, string confirmpassword)
        {
            ViewBag.Email = email;
            ViewBag.Token = token;

            if (newpassword.Length <= 4)
            {
                ViewBag.Error = "Password must have at least 5 characters.";
                return View();
            }

            if (newpassword != confirmpassword)
            {
                ViewBag.Error = "Password mismatch.";
                return View();
            }

            var user = _userService.GetUser(email);
            if (user == null)
                return View("InvalidLink");

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
