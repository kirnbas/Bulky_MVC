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
using Microsoft.AspNetCore.Identity;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IUnitOfWork unitOfWork, 
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM model = new RoleManagementVM();

            model.Roles = _roleManager.Roles.Select(x => new SelectListItem(x.Name, x.Name));
            model.Companies = _unitOfWork.Company.GetAll().Select(x => new SelectListItem(x.Name, x.Id.ToString()));
            
            model.User = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties:"Company");
            model.User.Role = _userManager.GetRolesAsync(model.User).GetAwaiter().GetResult().FirstOrDefault();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RoleManagement(RoleManagementVM model)
        {
            var user = _unitOfWork.ApplicationUser.Get(x => x.Id == model.User.Id, tracked: true);

            var userRole = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
            _userManager.RemoveFromRoleAsync(user, userRole).GetAwaiter().GetResult();
            _userManager.AddToRoleAsync(user, model.User.Role).GetAwaiter().GetResult();

            var role = _roleManager.Roles.FirstOrDefault(x => x.Name == model.User.Role);
            if (role.Name == SD.Role_Company)
            {
                user.CompanyId = model.User.CompanyId;
            }
            else
            {
                user.CompanyId = null;
            }
            _unitOfWork.Save();

            TempData["success"] = "Role successfully changed";

            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var applicationUsers = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();

            foreach (var user in applicationUsers)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

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
            var objFromDb = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
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
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }

        #endregion
    }
}
