using BulkyWebRazor_Temp.Data;
using BulkyWebRazor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkyWebRazor_Temp.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        [BindProperty]
        public Category? Category { get; set; }

        public DeleteModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet(int? id)
        {
            if (id == null || id == 0) {
                throw new ArgumentNullException(nameof(id));
            }

            Category = _db.Categories.Find(id);

            if (Category == null)
            {
                throw new ArgumentNullException(nameof(id));
            }
        }

        public IActionResult OnPost()
        {
            if (Category == null)
            {
                return NotFound();
            }

            _db.Categories.Remove(Category);
            _db.SaveChanges();

            TempData["success"] = "Deleted successfully";

            return RedirectToPage("Index");
        }
    }
}
