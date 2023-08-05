using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //var companies = unitOfWork.Company.GetAll();
            return View();
        }

        public IActionResult Upsert(int? id) // UpdateInsert
        {

            if (id == null || id == 0) //update
            {                
                return View(new Company());
            }
            else
            {
                var model = unitOfWork.Company.Get(p => p.Id == id);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    unitOfWork.Company.Add(model);
                }
                else
                {
                    unitOfWork.Company.Update(model);
                }

                unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            
            return View(model);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var companies = unitOfWork.Company.GetAll().ToList();
            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = unitOfWork.Company.Get(u => u.Id == id);

            unitOfWork.Company.Remove(companyToBeDeleted);
            unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
