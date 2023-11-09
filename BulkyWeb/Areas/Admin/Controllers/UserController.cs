using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BulkyBook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM model = new RoleManagementVM();

            model.Roles = _db.Roles.Select(x => new SelectListItem(x.Name, x.Id));
            model.Companies = _db.Companies.Select(x => new SelectListItem(x.Name, x.Id.ToString()));

            model.User = _db.ApplicationUsers.FirstOrDefault(u => u.Id == userId);
            var userRole = _db.UserRoles.FirstOrDefault(x => x.UserId == userId);
            model.User.Role = model.Roles.FirstOrDefault(u => u.Value == userRole.RoleId).Value;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RoleManagement(RoleManagementVM model)
        {
            var userRole = _db.UserRoles.FirstOrDefault(x => x.UserId == model.User.Id);
            _db.UserRoles.Remove(userRole);
            _db.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>()
            {
                UserId = model.User.Id,
                RoleId = model.User.Role
            });

            var user = _db.ApplicationUsers.FirstOrDefault(x => x.Id == model.User.Id);

            var role = _db.Roles.FirstOrDefault(x => x.Id == model.User.Role);
            if (role.Name == SD.Role_Company)
            {
                user.CompanyId = model.User.CompanyId;
            }
            else
            {
                user.CompanyId = null;
            }
            _db.SaveChanges();

            TempData["success"] = "Role successfully changed";

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var applicationUsers = _db.ApplicationUsers.Include(u => u.Company).ToList();

            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();

            foreach (var user in applicationUsers)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

                if (user.Company == null)
                {
                    user.Company = new() { Name = "" };
                }
            }

            return Json(new { data = applicationUsers });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unlocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                // user is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
