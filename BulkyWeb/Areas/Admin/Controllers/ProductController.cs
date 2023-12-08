using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var products = unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(products);
        }

        public IActionResult Upsert(int? id) // UpdateInsert
        {

            IEnumerable<SelectListItem> categoryList = unitOfWork.Category.GetAll()
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()));

            //ViewBag.CategoryList = categoryList;
            //ViewData["CategoryList"] = categoryList;
            var model = new ProductVM
            {
                CategoryList = categoryList,
            };

            if (id == null || id == 0)
            {
                // create
                model.Product = new Product();
            }
            else
            {
                //update
                model.Product = unitOfWork.Product.Get(p => p.Id == id, includeProperties: "ProductImages");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM model, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                if (model.Product.Id == 0)
                {
                    unitOfWork.Product.Add(model.Product);
                }
                else
                {
                    unitOfWork.Product.Update(model.Product);
                }

                unitOfWork.Save();

                string wwwRootPath = webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath =  @"images\products\product-" + model.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = model.Product.Id
                        };

                        if (model.Product.ProductImages == null)
                            model.Product.ProductImages = new List<ProductImage>();

                        model.Product.ProductImages.Add(productImage);
                    }

                    unitOfWork.Product.Update(model.Product);
                    unitOfWork.Save();
                }
                
                TempData["success"] = "Product created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                model.CategoryList = unitOfWork.Category.GetAll()
                .Select(c => new SelectListItem(c.Name, c.Id.ToString()));

                return View(model);
            }
        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }

        //    var product = unitOfWork.Product.Get(p => p.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //[HttpPost]
        //[ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public IActionResult DeletePOST(int id)
        //{
        //    var product = unitOfWork.Product.Get(p => p.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    unitOfWork.Product.Remove(product);
        //    unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";

        //    return RedirectToAction("Index");
        //}

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = unitOfWork.ProductImage.Get(x => x.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                        Path.Combine(webHostEnvironment.WebRootPath,
                        imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                unitOfWork.ProductImage.Remove(imageToBeDeleted);
                unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var products = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data= products });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = unitOfWork.Product.Get(u => u.Id == id);

            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }

            unitOfWork.Product.Remove(productToBeDeleted);
            unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }

        #endregion
    }
}
