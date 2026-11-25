using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using English_Vocabulary_Learning_Website.Models;

namespace English_Vocabulary_Learning_Website.Controllers
{
    public class AccountController : Controller
    {
        private VocabularyContext db = new VocabularyContext();

        // GET: Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(UserAccount user)
        {
            // 1. Kiểm tra dữ liệu nhập vào
            string error = null;

            if (user == null || string.IsNullOrWhiteSpace(user.Username)
                || string.IsNullOrWhiteSpace(user.Password) || string.IsNullOrWhiteSpace(user.Email))
            {
                error = "Vui lòng nhập đầy đủ thông tin!";
            }
            else if (user.Username.Trim().Length < 3)
            {
                error = "Tên đăng nhập phải có ít nhất 3 ký tự!";
            }
            else if (user.Password.Length < 6)
            {
                error = "Mật khẩu phải có ít nhất 6 ký tự!";
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(user.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                error = "Email không đúng định dạng!";
            }

            if (error != null)
            {
                ViewBag.error = error;
                return View(user);   // giữ lại những gì người dùng đã nhập
            }

            user.Username = user.Username.Trim();
            user.Email = user.Email.Trim();

            // 2. Kiểm tra trùng tên đăng nhập
            var check = db.Users.FirstOrDefault(s => s.Username == user.Username);
            if (check != null)
            {
                ViewBag.error = "Tên đăng nhập đã tồn tại!";
                return View(user);
            }

            // 3. Lưu tài khoản (luôn là User, không cho tự đăng ký Admin)
            user.Role = "User";
            user.CreatedAt = DateTime.Now;
            user.Password = PasswordHelper.Hash(user.Username, user.Password);
            db.Users.Add(user);
            db.SaveChanges();
            return RedirectToAction("Login");
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (ModelState.IsValid)
            {
                string hashed = PasswordHelper.Hash(username, password);
                var data = db.Users.FirstOrDefault(s => s.Username == username && s.Password == hashed);
                if (data != null)
                {
                    // Lưu thông tin người dùng vào Session
                    Session["UserId"] = data.Id;
                    Session["Username"] = data.Username;
                    Session["Role"] = data.Role;
                    return RedirectToAction("Index", "Learn");
                }
                else
                {
                    ViewBag.error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                    return View();
                }
            }
            return View();
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear(); // Xóa Session
            return RedirectToAction("Login");
        }
    }
}